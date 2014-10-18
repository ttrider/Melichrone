using System;
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
    class Program
    {
        static void Main(string[] args)
        {
            dynamic country = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(@"E:\src\Melichrone\metadata\usa.json"));

            var rules = country.rules;

            for (int i = 0; i < states.Length; i++)
            {
                var statecode = states[i];

                var stateBuffer = GetState(statecode);

                var name = stateName.Match(stateBuffer).Groups["stateName"].Value;

                Console.WriteLine(string.Format("{0}\t{1}", statecode, name));

                // code to name
                rules.Add(new JObject
                {
                    {"match", new JObject
                    {
                        { "common.geography.country.code", "USA" },
                        { "common.geography.state.code", statecode }
                    }}, 
                    {"produce", new JObject
                    {
                        { "common.geography.state.name", name }
                    }}
                });



                // name to code
                rules.Add(new JObject
                {
                    {"match", new JObject
                    {
                        { "common.geography.country.code", "USA" },
                        { "common.geography.state.name", name }
                    }}, 
                    {"produce", new JObject
                    {
                        { "common.geography.state.code", statecode },
                        { "common.geography.state.nickname", stateNickname.Match(stateBuffer).Groups["value"].Value},
                        { "common.geography.state.capital", stateCapital.Match(stateBuffer).Groups["value"].Value},
                        { "common.geography.state.largestCity", stateLargest.Match(stateBuffer).Groups["value"].Value},
                        { "common.geography.state.population", statePopulation.Match(stateBuffer).Groups["value"].Value.Replace(",","")},
                        { "common.geography.state.area", stateArea.Match(stateBuffer).Groups["value"].Value.Replace(",","")},
                        { "common.geography.state.borderingStates", GetBorderingStates(stateBuffer)},
                    }}
                });


                

            }

            File.WriteAllText(@"E:\src\Melichrone\metadata\usa.json", JsonConvert.SerializeObject(country, Formatting.Indented));
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

        static Regex stateName = new Regex(@"<big>(?<stateName>.*)</big>", RegexOptions.Compiled);

        static Regex stateNickname = new Regex(@"<td>State nickname</td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);
        static Regex stateCapital = new Regex(@"<td>Capital</td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);
        static Regex stateLargest = new Regex(@"<td>Largest City</td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);
        static Regex statePopulation = new Regex(@"<td>Population <sub>\(2013\)</sub></td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);
        static Regex stateArea = new Regex(@"<td>Area \(sq. mi.\)</td><td>(?<value>[^<]*)</td>", RegexOptions.Compiled);

        static Regex stateBorderingStates = new Regex(@"(<td>Bordering states</td><td>)|(<a href=""/\w\w/"">(?<state>[^<]*)</a>)+", RegexOptions.Compiled);

        
        
        static Regex stateCities = new Regex(@"<li><a href=""/(?<url>[^/]*)/livability/"">(?<name>[^<]*)</a></li>", RegexOptions.Compiled);
        



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
