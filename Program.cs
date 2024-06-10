using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace HackerRankRestApi
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await Task.Run(() => GetMathcesDrawn());
            await Task.Run(() => GetWinnerTotalGoals());

            Console.ReadLine();
        }

        #region Matches Drawn

        public static async Task GetMathcesDrawn()
        {
            while (true)
            {
                Console.Write("Enter the year for all drawn results: ");
                string input = Console.ReadLine();
                int year = 0;
                bool isTrue = int.TryParse(input, out year);
                using (HttpClient client = new HttpClient())
                {
                    string baseAddress = $"https://jsonmock.hackerrank.com/api/football_matches";
                    //string year = "2012";
                    int totalPages = -1;
                    int matchesDrawn = 0;
                    List<string> allCompetitions = new List<string>();
                    int totalMatchesPlayed = 0;

                    string url = $"{baseAddress}?year={year}";
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var json = JsonConvert.DeserializeObject<Rootobject>(result);

                        totalPages = json.total_pages;
                        for (int i = 1; i <= totalPages; i++)
                        {
                            string matchPerPages = $"{url}&page={i}";
                            var mathcesPerPage = await client.GetAsync(matchPerPages);
                            if (mathcesPerPage.IsSuccessStatusCode)
                            {
                                var resultPerMatch = await mathcesPerPage.Content.ReadAsStringAsync();
                                var jsonMatch = JsonConvert.DeserializeObject<Rootobject>(resultPerMatch);
                                object[] array = jsonMatch.data;
                                totalMatchesPlayed = totalMatchesPlayed + array.Count();
                                foreach (var item in array)
                                {
                                    var resultOfMatch = JsonConvert.DeserializeObject<Competition>(item.ToString());
                                    if (!allCompetitions.Contains(resultOfMatch.competition))
                                    {
                                        allCompetitions.Add(resultOfMatch.competition);
                                    }
                                    if (resultOfMatch.team1goals == resultOfMatch.team2goals)
                                    {
                                        matchesDrawn++;
                                    }
                                }
                            }
                        }
                    }

                    Console.WriteLine("Name of all Competitions: ");
                    foreach (var item in allCompetitions)
                    {
                        Console.WriteLine(item);
                    }
                    Console.WriteLine();
                    Console.WriteLine("Matches Drawn: {0}", matchesDrawn.ToString());
                    Console.WriteLine("Total Matches: {0} \n", totalMatchesPlayed.ToString());
                }
            }
        }


        public class Rootobject
        {
            public int page { get; set; }
            public int per_page { get; set; }
            public int total { get; set; }
            public int total_pages { get; set; }
            public object[] data { get; set; }
        }


        public class Competition
        {
            public string competition { get; set; }
            public int year { get; set; }
            public string round { get; set; }
            public string team1 { get; set; }
            public string team2 { get; set; }
            public string team1goals { get; set; }
            public string team2goals { get; set; }
        }


        #endregion

        #region Winner's Goal

        public static async Task GetWinnerTotalGoals()
        {
            while (true)
            {
                using (HttpClient client = new HttpClient())
                {
                    //--https://jsonmock.hackerrank.com/api/football_competitions?name=UEFA Champions League&year=2013
                    string competition = "UEFA Champions League";
                    Console.Write("Enter year of Champions League: \n");
                    string year = Console.ReadLine();
                    string baseAddressCompetitionYear = "https://jsonmock.hackerrank.com/api/football_competitions";
                    string url_Competition_Year = $"{baseAddressCompetitionYear}?name={competition}&year={year}";
                    int totalGoalsScoredbyWinner = 0;
                    string winnerName = string.Empty;

                    var responeWInnerInYear = await client.GetAsync(url_Competition_Year);
                    if (responeWInnerInYear.IsSuccessStatusCode)
                    {
                        var readWinnerinYear = await responeWInnerInYear.Content.ReadAsStringAsync();
                        var json = JsonConvert.DeserializeObject<Rootobject>(readWinnerinYear);
                        CompetitionWinnerByYear winnerObj = new CompetitionWinnerByYear();
                        foreach (var item in json.data)
                        {
                            winnerObj = JsonConvert.DeserializeObject<CompetitionWinnerByYear>(item.ToString());
                        }

                        winnerName = winnerObj.winner;

                        string baseAddressMatches = "https://jsonmock.hackerrank.com/api/football_matches";
                        string homeGoals = $"{baseAddressMatches}?competition={competition}&year={year}&team1={winnerName}&page={1}";
                        string awayGoals = $"{baseAddressMatches}?competition={competition}&year={year}&team2={winnerName}&page={1}";

                        int homeGoalsCount = await GetGoals(client, homeGoals, true);
                        int awayGoalsCount = await GetGoals(client, awayGoals, false);

                        totalGoalsScoredbyWinner = homeGoalsCount + awayGoalsCount;
                    }

                    Console.WriteLine("Competition: {0}", competition);
                    Console.WriteLine("Winner: {0}", winnerName);
                    Console.WriteLine("Winning Year: {0}", competition, year, totalGoalsScoredbyWinner);
                    Console.WriteLine("Totals Goals: {0} \n", totalGoalsScoredbyWinner);
                }
            }
        }

        public static async Task<int> GetGoals(HttpClient client, string urlGoals, bool team1)
        {
            int goals = 0;
            var resultHomeGoals = await client.GetAsync(urlGoals);
            if (resultHomeGoals.IsSuccessStatusCode)
            {
                var readHomeGoals = await resultHomeGoals.Content.ReadAsStringAsync();
                var jsonHomeGoals = JsonConvert.DeserializeObject<Rootobject>(readHomeGoals);
                var objectHomeGoal = jsonHomeGoals.data;

                foreach (var item in objectHomeGoal)
                {
                    var resultHome = JsonConvert.DeserializeObject<Competition>(item.ToString());

                    goals = team1 ? goals + Convert.ToInt32(resultHome.team1goals) : goals + Convert.ToInt32(resultHome.team2goals);
                }
            }

            return goals;
        }


        public class CompetitionWinnerByYear
        {
            public string name { get; set; }
            public string country { get; set; }
            public int year { get; set; }
            public string winner { get; set; }
            public string runnerup { get; set; }
        }

        #endregion
    }
}
