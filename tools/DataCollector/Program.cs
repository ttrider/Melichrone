using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
            ProcessAllZipFiles_details();
        }

        private static void ProcessAllZipFiles()
        {
            var hispanicRegex = new Regex(@"Hispanic/Latino:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var whiteRegex = new Regex(@"White\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var blackRegex = new Regex(@"Black\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var nativeRegex = new Regex(@"Native American\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var asianRegex = new Regex(@"Asian\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var hawaRegex =
                new Regex(@"Hawaiian/Pacific Islander\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                    RegexOptions.Compiled | RegexOptions.Multiline);
            var otherRegex = new Regex(@"Other\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var mixedRegex = new Regex(@"Multiracial\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var latitudeRegex = new Regex(@"Latitude:\s*</td><td align=""right"" class=""text"">(?<value>[\d.-]*)<",
                RegexOptions.Multiline);
            var longtitudeRegex = new Regex(@"Longitude:\s*</td><td align=""right"" class=""text"">(?<value>[\d.-]*)<",
                RegexOptions.Multiline);
            var populationRegex = new Regex(@"Population:\s*</td><td align=""right"" class=""text"">(?<value>[\d.-]*)<",
                RegexOptions.Multiline);
            var neverMarriedRegex = new Regex(
                @"Never married:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<", RegexOptions.Multiline);
            var marriedRegex = new Regex(@"Married:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var separatedRegex = new Regex(@"Separated:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var widowedRegex = new Regex(@"Widowed:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var divorcedRegex = new Regex(@"Divorced:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var sexRegex = new Regex(
                @"All Ages:.+?Male:[^\d]*(?<allmaleages>[\d.]+)%.+?Female:[^\d]*(?<allfemaleages>[\d.]+)%",
                RegexOptions.Multiline);
            var cityStateRegex = new Regex(@"class=""place_title"">\s*\((?<city>[\s\w]*?)\s(?<state>\w\w)\)",
                RegexOptions.Multiline);

            var ageSexRegex =
                new Regex(
                    @"<nobr>(?<gr>[^:]*):</nobr>&nbsp;</td><td align=""right"" class=""text"">(?<male>[^%]*)%[^\d]*(?<maleall>[^%]*)%[^\d]*(?<female>[^%]*)%[^\d]*(?<femaleall>[^%]*)%[^\d]*(?<bothall>[^%]*)%",
                    RegexOptions.Multiline);


            var dir = new DirectoryInfo(@"E:\temp\zipdata");

            dynamic d = new JObject
            {
                {"rules", new JArray()}
            };


            dynamic rules = d.rules;



            var states = new Dictionary<string, Dictionary<string, JObject>>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileInfo in dir.EnumerateFiles("*.html").Skip(3))
            {
                Console.WriteLine("{0}\t{1}", Environment.WorkingSet / 1024 / 1024, fileInfo.FullName);

                var zip = fileInfo.Name.Substring(0, 5);

                var buffer = File.ReadAllText(fileInfo.FullName);

                try
                {
                    var hispanic = double.Parse(hispanicRegex.Match(buffer).Groups["value"].Value);
                    var white = double.Parse(whiteRegex.Match(buffer).Groups["value"].Value);
                    var black = double.Parse(blackRegex.Match(buffer).Groups["value"].Value);
                    var native = double.Parse(nativeRegex.Match(buffer).Groups["value"].Value);
                    var asian = double.Parse(asianRegex.Match(buffer).Groups["value"].Value);
                    var hawa = double.Parse(hawaRegex.Match(buffer).Groups["value"].Value);
                    var other = double.Parse(otherRegex.Match(buffer).Groups["value"].Value);
                    var mixed = double.Parse(mixedRegex.Match(buffer).Groups["value"].Value);
                    var latitude = double.Parse(latitudeRegex.Match(buffer).Groups["value"].Value);
                    var longtitude = double.Parse(longtitudeRegex.Match(buffer).Groups["value"].Value);
                    var population = double.Parse(populationRegex.Match(buffer).Groups["value"].Value);


                    double neverMarried = 0;
                    double.TryParse(neverMarriedRegex.Match(buffer).Groups["value"].Value, out neverMarried);
                    double married = 0;
                    double.TryParse(marriedRegex.Match(buffer).Groups["value"].Value, out married);
                    double separated = 0;
                    double.TryParse(separatedRegex.Match(buffer).Groups["value"].Value, out separated);
                    double widowed = 0;
                    double.TryParse(widowedRegex.Match(buffer).Groups["value"].Value, out widowed);
                    double divorced = 0;
                    double.TryParse(divorcedRegex.Match(buffer).Groups["value"].Value, out divorced);


                    int males = 0;
                    int females = 0;
                    var match = sexRegex.Match(buffer);
                    if (match.Success)
                    {
                        males = (int)(double.Parse(match.Groups["allmaleages"].Value) * 10);
                        females = (int)(double.Parse(match.Groups["allfemaleages"].Value) * 10);
                    }

                    string state = "";
                    string city = "";
                    match = cityStateRegex.Match(buffer);
                    if (match.Success)
                    {
                        city = match.Groups["city"].Value.ToLower();
                        state = match.Groups["state"].Value.ToLower();
                    }


                    //Console.WriteLine(zip);
                    //Console.WriteLine(hispanic);
                    //Console.WriteLine(white);
                    //Console.WriteLine(black);
                    //Console.WriteLine(native);
                    //Console.WriteLine(asian);
                    //Console.WriteLine(hawa);
                    //Console.WriteLine(other);
                    //Console.WriteLine(mixed);
                    //Console.WriteLine(latitude);
                    //Console.WriteLine(longtitude);
                    //Console.WriteLine(population);
                    //Console.WriteLine(neverMarried);
                    //Console.WriteLine(married);
                    //Console.WriteLine(separated);
                    //Console.WriteLine(widowed);
                    //Console.WriteLine(divorced);
                    //Console.WriteLine(males);
                    //Console.WriteLine(females);

                    //Console.WriteLine(city);
                    //Console.WriteLine(state);


                    Dictionary<string, JObject> stateinfo;
                    if (!states.TryGetValue(state, out stateinfo))
                    {
                        stateinfo = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
                        states[state] = stateinfo;
                    }


                    JObject cityinfo;
                    if (!stateinfo.TryGetValue(city, out cityinfo))
                    {
                        cityinfo = new JObject
                        {

                        };

                        var dd = new JObject
                        {
                            {
                                "match", new JObject
                                {
                                    {"common.geography.country.code", "USA"},
                                    {"common.geography.state.code", state},
                                    {"common.geography.city.name", city}
                                }
                            },
                            {
                                "produce", new JObject
                                {
                                    {"common.geography.zip.code", cityinfo}
                                }
                            }
                        };

                        rules.Add(dd);

                        stateinfo[city] = cityinfo;
                    }

                    cityinfo.Add(zip, (int)population);

                }
                catch (Exception)
                {
                    Console.WriteLine("no data");

                }
            }

            File.WriteAllText(@"..\..\..\..\metadata\usa.zipcodes.json", JsonConvert.SerializeObject(d, Formatting.Indented));
        }

        private static void ProcessAllZipFiles_details()
        {
            var hispanicRegex = new Regex(@"Hispanic/Latino:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var whiteRegex = new Regex(@"White\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var blackRegex = new Regex(@"Black\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var nativeRegex = new Regex(@"Native American\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var asianRegex = new Regex(@"Asian\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var hawaRegex =
                new Regex(@"Hawaiian/Pacific Islander\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                    RegexOptions.Compiled | RegexOptions.Multiline);
            var otherRegex = new Regex(@"Other\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var mixedRegex = new Regex(@"Multiracial\*:&nbsp;</td><td align=""right"" class=""text"">(?<value>[\d.]*)",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var latitudeRegex = new Regex(@"Latitude:\s*</td><td align=""right"" class=""text"">(?<value>[\d.-]*)<",
                RegexOptions.Multiline);
            var longtitudeRegex = new Regex(@"Longitude:\s*</td><td align=""right"" class=""text"">(?<value>[\d.-]*)<",
                RegexOptions.Multiline);
            var populationRegex = new Regex(@"Population:\s*</td><td align=""right"" class=""text"">(?<value>[\d.-]*)<",
                RegexOptions.Multiline);
            var neverMarriedRegex = new Regex(
                @"Never married:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<", RegexOptions.Multiline);
            var marriedRegex = new Regex(@"Married:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var separatedRegex = new Regex(@"Separated:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var widowedRegex = new Regex(@"Widowed:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var divorcedRegex = new Regex(@"Divorced:\s*</td><td align=""right"" class=""[^""]+"">(?<value>[\d.-]*)%<",
                RegexOptions.Multiline);
            var sexRegex = new Regex(
                @"All Ages:.+?Male:[^\d]*(?<allmaleages>[\d.]+)%.+?Female:[^\d]*(?<allfemaleages>[\d.]+)%",
                RegexOptions.Multiline);
            var cityStateRegex = new Regex(@"class=""place_title"">\s*\((?<city>[\s\w]*?)\s(?<state>\w\w)\)",
                RegexOptions.Multiline);

            var ageSexRegex =
                new Regex(
                    @"<nobr>(?<gr>[^:]*):</nobr>&nbsp;</td><td align=""right"" class=""text"">(?<male>[^%]*)%[^\d]*(?<maleall>[^%]*)%[^\d]*(?<female>[^%]*)%[^\d]*(?<femaleall>[^%]*)%[^\d]*(?<bothall>[^%]*)%",
                    RegexOptions.Multiline);


            var dir = new DirectoryInfo(@"E:\temp\zipdata");

            dynamic d = new JObject
            {
                {"rules", new JArray()}
            };


            dynamic rules = d.rules;



            var states = new Dictionary<string, Dictionary<string, JObject>>(StringComparer.OrdinalIgnoreCase);

            foreach (var fileInfo in dir.EnumerateFiles("*.html").Skip(3))
            {
                Console.WriteLine("{0}\t{1}", Environment.WorkingSet / 1024 / 1024, fileInfo.FullName);

                var zip = fileInfo.Name.Substring(0, 5);

                var buffer = File.ReadAllText(fileInfo.FullName);

                try
                {
                    var hispanic = (int)(double.Parse(hispanicRegex.Match(buffer).Groups["value"].Value) * 10);
                    var white = (int)(double.Parse(whiteRegex.Match(buffer).Groups["value"].Value) * 10);
                    var black = (int)(double.Parse(blackRegex.Match(buffer).Groups["value"].Value) * 10);
                    var native = (int)(double.Parse(nativeRegex.Match(buffer).Groups["value"].Value) * 10);
                    var asian = (int)(double.Parse(asianRegex.Match(buffer).Groups["value"].Value) * 10);
                    var hawa = (int)(double.Parse(hawaRegex.Match(buffer).Groups["value"].Value) * 10);
                    var other = (int)(double.Parse(otherRegex.Match(buffer).Groups["value"].Value) * 10);
                    var mixed = (int)(double.Parse(mixedRegex.Match(buffer).Groups["value"].Value) * 10);
                    var latitude = double.Parse(latitudeRegex.Match(buffer).Groups["value"].Value);
                    var longtitude = double.Parse(longtitudeRegex.Match(buffer).Groups["value"].Value);
                    var population = double.Parse(populationRegex.Match(buffer).Groups["value"].Value);


                    double neverMarried = 0;
                    double.TryParse(neverMarriedRegex.Match(buffer).Groups["value"].Value, out neverMarried);
                    double married = 0;
                    double.TryParse(marriedRegex.Match(buffer).Groups["value"].Value, out married);
                    double separated = 0;
                    double.TryParse(separatedRegex.Match(buffer).Groups["value"].Value, out separated);
                    double widowed = 0;
                    double.TryParse(widowedRegex.Match(buffer).Groups["value"].Value, out widowed);
                    double divorced = 0;
                    double.TryParse(divorcedRegex.Match(buffer).Groups["value"].Value, out divorced);


                    int males = 0;
                    int females = 0;
                    var match = sexRegex.Match(buffer);
                    if (match.Success)
                    {
                        males = (int)(double.Parse(match.Groups["allmaleages"].Value) * 10);
                        females = (int)(double.Parse(match.Groups["allfemaleages"].Value) * 10);
                    }

                    string state = "";
                    string city = "";
                    match = cityStateRegex.Match(buffer);
                    if (match.Success)
                    {
                        city = match.Groups["city"].Value.ToLower();
                        state = match.Groups["state"].Value.ToLower();
                    }


                    //Console.WriteLine(zip);
                    //Console.WriteLine(hispanic);
                    //Console.WriteLine(white);
                    //Console.WriteLine(black);
                    //Console.WriteLine(native);
                    //Console.WriteLine(asian);
                    //Console.WriteLine(hawa);
                    //Console.WriteLine(other);
                    //Console.WriteLine(mixed);
                    //Console.WriteLine(latitude);
                    //Console.WriteLine(longtitude);
                    //Console.WriteLine(population);
                    //Console.WriteLine(neverMarried);
                    //Console.WriteLine(married);
                    //Console.WriteLine(separated);
                    //Console.WriteLine(widowed);
                    //Console.WriteLine(divorced);
                    //Console.WriteLine(males);
                    //Console.WriteLine(females);

                    //Console.WriteLine(city);
                    //Console.WriteLine(state);

                    var dd = new JObject
                    {
                        {
                            "match", new JObject
                            {
                                {"common.geography.country.code", "USA"},
                                {"common.geography.state.zip.code", zip}
                            }
                        },
                        {
                            "produce", new JObject
                            {
                                {"common.geography.zip.population", (int)population},
                                {"common.geography.zip.latitude", latitude},
                                {"common.geography.zip.longtitude", longtitude},
                                {
                                    "common.person.gender", new JObject
                                    {
                                        {"male", males},
                                        {"female", females},
                                    }
                                },
                                {
                                    "common.person.race", new JObject
                                    {
                                            {"hispanic",hispanic},
                                            {"white",  white},
                                            {"black",  black},
                                            {"native", native},
                                            {"asian",  asian},
                                            {"pacific",  hawa},
                                            {"mixed",  mixed},
                                            {"*",other}
                                    }
                                }
                            }
                        }
                    };

                    rules.Add(dd);

                }
                catch (Exception)
                {
                    Console.WriteLine("no data");

                }
            }

            File.WriteAllText(@"..\..\..\..\metadata\usa.zipcode_info.json", JsonConvert.SerializeObject(d, Formatting.Indented));
        }

        private static void ZipFileProcessing()
        {
            var datafile = @"E:\src\Melichrone\metadata\zip_code_database.csv";

            dynamic d = new JObject
            {
                {"rules", new JArray()}
            };


            dynamic rules = d.rules;


            var ac = new Dictionary<string, JObject>();


            foreach (var readLine in File.ReadLines(datafile).Skip(1))
            {
                // parse line
                //"zip","type","primary_city","acceptable_cities","unacceptable_cities","state","county","timezone","area_codes","latitude","longitude","world_region","country","decommissioned","estimated_population","notes"


                var data = parseZipFileLine(readLine).ToArray();

                var zip = data[0];
                var city = data[2];
                var state = data[5];
                var areacodes = data[8];

                int pop;
                if (!int.TryParse(data[14].Replace(".", "").Replace(",", ""), out pop))
                {
                    pop = 0;
                }


                var aj = new JObject();
                foreach (var areacode in areacodes.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(areacode))
                    {
                        JObject o;
                        if (!ac.TryGetValue(areacode, out o))
                        {
                            o = new JObject();
                            ac[areacode] = o;
                        }
                        o.Add(zip, pop);
                    }
                }

                Console.WriteLine("{0}\t{1}\t{2}\t{3}", zip, city, state, areacodes);
            }

            foreach (var areazip in ac)
            {
                rules.Add(new JObject
                {
                    {
                        "match", new JObject
                        {
                            {"common.geography.country.code", "USA"},
                            {"common.geography.areacode", areazip.Key}
                        }
                    },
                    {
                        "produce", new JObject
                        {
                            {"common.geography.zip", areazip.Value}
                        }
                    }
                });
            }


            File.WriteAllText(@"..\..\..\..\metadata\usa.areacode_zip.json", JsonConvert.SerializeObject(d, Formatting.Indented));
        }

        private static IEnumerable<string> parseZipFileLine(string line)
        {
            Regex someRegex = new Regex(@"(\""(?<val>[^\""]+)\""\,?)|(\,)", RegexOptions.Compiled);

            var match = someRegex.Match(line);
            while (match.Success)
            {
                if (match.Groups["val"].Success)
                {
                    yield return match.Groups["val"].Value;
                }
                else
                {
                    yield return string.Empty;
                }
                match = match.NextMatch();
            }
        }



        private static IEnumerable<string> ScanZip()
        {
            Regex someRegex = new Regex(@"<a href=""http://www.zipskinny.com/index.php\?zip=(?<zip>\d\d\d\d\d)"" class=""text_link"">(?<value>[^<]*)</a>", RegexOptions.Compiled | RegexOptions.Multiline);


            foreach (var state in states)
            {
                Console.WriteLine(state);

                var client = new WebClient();
                client.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
                client.Headers.Add("Accept-Language", "en-US,en;q=0.8,ru;q=0.6");
                client.Headers.Add("Cache-Control", "max-age=0");
                client.Headers.Add("Cookie", "__qca=P0-2083547077-1414171717982");
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/38.0.2125.104 Safari/537.36");





                var buffer = client.DownloadString("http://www.zipskinny.com/state.php?state=" + state);

                var match = someRegex.Match(buffer);
                while (match.Success)
                {
                    var zip = match.Groups["zip"].Value;
                    var value = match.Groups["value"].Value.ToLower();

                    var ret = string.Format("{0}\t{1}\t{2}", state, zip, value);
                    Console.WriteLine(ret);
                    yield return ret;
                    match = match.NextMatch();
                }
            }
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
                catch (Exception ex)
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
