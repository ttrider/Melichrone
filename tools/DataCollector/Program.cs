using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataCollector
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ScanLastNames();
        }


        static string[] origins = {
    "African",
    "American",
    "Arabic",
    "Armenian",
    "Catalan",
    "Chinese",
    "Cornish",
    "Czech",
    "Danish",
    "Dutch",
    "English",
    "Finnish",
    "French",
    "Galician",
    "German",
    "Greek",
    "Hungarian",
    "Indian",
    "Irish",
    "Italian",
    "Japanese",
    "Jewish",
    "Korean",
    "Lithuanian",
    "Muslim",
    "Native American",
    "Norwegian",
    "Polish",
    "Portuguese",
    "Russian",
    "Scandinavian",
    "Scottish",
    "Slavic",
    "Spanish",
    "Swedish",
    "Swiss",
    "Turkish",
    "Ukrainian",
    "Vietnamese",
    "Welsh"
    };


        private static void ScanLastNames()
        {
            dynamic d = new JObject
            {
                {"rules",new JArray()}
            };


            dynamic rules = d.rules;



            foreach (var origin in origins)
            {
                try
                {
                    var url = @"http://genealogy.familyeducation.com/browse/origin/" + origin;

                    var names = new JArray();

                    rules.Add(new JObject
                    {
                        {
                            "match", new JObject
                            {
                                {"common.person.origin", origin}
                            }
                        },
                        {
                            "produce", new JObject
                            {
                                {"common.person.familyName", names}
                            }
                        }
                    });


                    var page = 1;
                    var targetUrl = url;
                    while (true)
                    {
                        var client = new WebClient();

                        Console.WriteLine(targetUrl);

                        var data = client.DownloadString(targetUrl);

                        var match = lastNames.Match(data);
                        while (match.Success)
                        {
                            var name = match.Groups["name"].Value.Trim();

                            names.Add(name);

                            match = match.NextMatch();
                        }

                        if (!data.Contains("next"))
                        {
                            break;
                        }

                        page++;
                        targetUrl = url + "/?page=" + page;
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            //File
            File.WriteAllText(@"..\..\..\..\metadata\familyNames.json",
                JsonConvert.SerializeObject(d, Formatting.Indented));

        }





        private static void ReScanCities()
        {
            var cities =
                File.ReadLines(@"C:\temp\Mel.log")
                    .Select(line => line.Split('\t'))
                    .Select(line => new { State = line[2], City = line[1], Url = line[0] });

            foreach (var city in cities)
            {
                Console.WriteLine("{0}\t{1}\thttp://www.areavibes.com/{2}/demographics/", city.State, city.City, city.Url);

                dynamic data = JsonConvert.DeserializeObject(File.ReadAllText(@"..\..\..\..\metadata\usa." + city.State + ".json",
                    Encoding.UTF8));

                var rules = data.rules;

                try
                {
                    rules.Add(GetCityValue(city.Url, city.City, city.State));
                }
                catch (Exception ex)
                {
                    var message = string.Format("{0}\t{1}\t{2}\t{3}\r\n", city.Url, city.City, city.State, ex.Message);

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine();
                    Console.WriteLine(message);
                    Console.ResetColor();

                    File.AppendAllText(@"c:\temp\Mel2.log", message);
                }


                File.WriteAllText(@"..\..\..\..\metadata\usa." + city.State + ".json",
                    JsonConvert.SerializeObject(data, Formatting.Indented));
            }

        }


        private static void ScanAllCities()
        {
            for (var i = 0; i < states.Length; i++)
            {
                var statecode = states[i];

                Console.WriteLine("{0}\t{1}", i, statecode);

                var data = new JObject();
                var rules = new JArray();
                data.Add("rules", rules);

                var stateBuffer = GetState(statecode);

                var requests = new BlockingCollection<Tuple<string, string, string>>();
                var responses = new BlockingCollection<JObject>();

                var loaders = new List<Task>();
                for (var j = 0; j < 3; j++)
                {
                    loaders.Add(Task.Run(() =>
                    {
                        foreach (var request in requests.GetConsumingEnumerable())
                        {
                            Exception error = null;
                            for (int k = 0; k < 5; k++)
                            {
                                try
                                {
                                    responses.Add(GetCityValue(request.Item1, request.Item2, request.Item3));
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    error = ex;
                                }
                            }

                            if (error != null)
                            {
                                var message = string.Format("{0}\t{1}\t{2}\t{3}\r\n", request.Item1, request.Item2,
                                    request.Item3, error.Message);

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine();
                                Console.WriteLine(message);
                                Console.ResetColor();

                                File.AppendAllText(@"c:\temp\Mel.log", message);
                            }
                        }
                    }));
                }

                Task.Factory.ContinueWhenAll(loaders.ToArray(), (t) => responses.CompleteAdding());


                var match = stateCities.Match(stateBuffer);
                while (match.Success)
                {
                    var url = match.Groups["url"].Value;
                    var name = match.Groups["name"].Value;

                    requests.Add(new Tuple<string, string, string>(url, name, statecode));

                    match = match.NextMatch();
                }
                requests.CompleteAdding();

                foreach (var response in responses.GetConsumingEnumerable())
                {
                    rules.Add(response);
                }

                File.WriteAllText(@"..\..\..\..\metadata\usa." + statecode + ".json",
                    JsonConvert.SerializeObject(data, Formatting.Indented));
            }
        }

        private static JObject GetCityValue(string url, string name, string statecode)
        {
            Console.Write(".");


            var cityBuffer = GetCity(url);

            var pop = cityPopulation.Match(cityBuffer).Groups["value"].Value.Replace(",", "");

            var mf = cityMaleFemale.Match(cityBuffer)
                .Groups["value"].Value
                .Split(':').Select(v =>
                {
                    var val = 1.0;
                    if (!double.TryParse(v, out val))
                    {
                        val = 1.0;
                    }
                    return (int)(val * 100);
                }).ToArray();


            var races = new JArray();

            var cm = cityRace.Match(cityBuffer);
            if (cm.Success)
            {
                var cmm = cityRaces.Match(cityBuffer, cm.Index);
                while (cmm.Success)
                {
                    var race = cmm.Groups["race"].Value;
                    var racep = (int)(double.Parse(cmm.Groups["value"].Value) * 100);

                    var ritem = new JObject();
                    ritem.Add(race, racep);

                    races.Add(ritem);


                    cmm = cmm.NextMatch();
                }
            }

            return new JObject
            {
                {
                    "match", new JObject
                    {
                        {"common.geography.country.code", "USA"},
                        {"common.geography.state.code", statecode},
                        {"common.geography.city.name", name}
                    }
                },
                {
                    "produce", new JObject
                    {
                        {"common.geography.city.population", pop},
                        {
                            "common.person.gender", new JObject
                            {
                                {"male", mf[0]},
                                {"female", mf[1]},
                            }
                        },
                        {
                            "common.person.race", races
                        }
                    }
                }
            };
        }


        static JArray GetBorderingStates(string stateBuffer)
        {
            var ret = new JArray();

            var match = stateBorderingStates.Match(stateBuffer);
            while (match.Success)
            {
                if (match.Groups["state"].Success)
                {
                    ret.Add(match.Groups["state"].Value);
                }


                match = match.NextMatch();
            }
            return ret;
        }


        static string GetState(string state)
        {
            var client = new WebClient();
            return client.DownloadString(string.Format("http://www.areavibes.com/{0}/", state));
        }
        static string GetCity(string c)
        {
            var client = new WebClient();
            return client.DownloadString(string.Format("http://www.areavibes.com/{0}/demographics/", c));
        }

        static Regex stateName = new Regex(@"<big>(?<stateName>.*)</big>", RegexOptions.Compiled);

        static Regex stateNickname = new Regex(@"<td>State nickname</td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);
        static Regex stateCapital = new Regex(@"<td>Capital</td><td>(<a href=""[^>]+>)?(?<value>[^<]+)(</a>)?", RegexOptions.Compiled);
        static Regex stateLargest = new Regex(@"<td>Largest City</td><td>(<a href=""[^>]+>)?(?<value>[^<]+)(</a>)?", RegexOptions.Compiled);
        static Regex statePopulation = new Regex(@"<td>Population <sub>\(2013\)</sub></td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);
        static Regex stateArea = new Regex(@"<td>Area \(sq. mi.\)</td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);

        static Regex stateBorderingStates = new Regex(@"(<td>Bordering states</td><td>)|(<a href=""/\w\w/"">(?<state>[^<]*)</a>)+", RegexOptions.Compiled);



        static Regex stateCities = new Regex(@"<li><a href=""/(?<url>[^/]*)/livability/"">(?<name>[^<]*)</a></li>", RegexOptions.Compiled);


        static Regex cityPopulation = new Regex(@"<td>Population<sub> \(2013\)</sub></td><td>(<a href=""[^>]+>)?(?<value>[^<]+)(</a>)?", RegexOptions.Compiled);
        static Regex cityMaleFemale = new Regex(@"<td>Male/Female ratio</td><td>(<a href=""[^>]+>)?(?<value>[^<]+)(</a>)?", RegexOptions.Compiled);


        static Regex lastNames = new Regex(@"<li>\s*<a href=""http://genealogy.familyeducation.com/surname-origin/[^""]+"">(?<name>[^<]*)");

        //race
        private static Regex cityRace = new Regex(@"(<tr><th>Race.+?</tr>)");

        private static Regex cityRaces = new Regex(@"(<tr><td>(?<race>[^<]*)</td><td>(?<value>[0-9.]*)%</td>.+?</tr>)");


        static readonly string[] states = new string[]
        {
            "DC",
          "AL",
          "AK",
          "AZ",
          "AR",
          "CA",
          "CO",
          "CT",
          "DE",
          "FL",
          "GA",
          "HI",
          "ID",
          "IL",
          "IN",
          "IA",
          "KS",
          "KY",
          "LA",
          "ME",
          "MD",
          "MA",
          "MI",
          "MN",
          "MS",
          "MO",
          "MT",
          "NE",
          "NV",
          "NH",
          "NJ",
          "NM",
          "NY",
          "NC",
          "ND",
          "OH",
          "OK",
          "OR",
          "PA",
          "RI",
          "SC",
          "SD",
          "TN",
          "TX",
          "UT",
          "VT",
          "VA",
          "WA",
          "WV",
          "WI",
          "WY"
        };
    }
}
