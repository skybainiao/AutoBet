using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SportsBettingClient
{
    public partial class MainWindow : Window
    {
        private const string Server1Url = "http://127.0.0.1:5000/matches";
        private const string Server2Url = "http://127.0.0.1:5001/matches";
        private readonly List<PairedMatchInfo> _pairedMatches = new();
        private readonly List<PairedMatchInfo> _betMatches = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void LoadServer1Data(object sender, RoutedEventArgs e)
        {
            var server1Data = await FetchServer1Data();
            if (server1Data != null)
            {
                var collectionView = new CollectionViewSource
                {
                    Source = server1Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Server1ListView.ItemsSource = collectionView.View;
            }
        }

        private async void LoadServer2Data(object sender, RoutedEventArgs e)
        {
            var server2Data = await FetchServer2Data();
            if (server2Data != null)
            {
                var collectionView = new CollectionViewSource
                {
                    Source = server2Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Server2ListView.ItemsSource = collectionView.View;
            }
        }

        private async Task<List<MatchInfo>> FetchServer1Data()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(Server1Url);

                var data = JsonSerializer.Deserialize<List<LeagueData>>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var matchList = new List<MatchInfo>();
                foreach (var league in data ?? new List<LeagueData>())
                {
                    foreach (var match in league.Events ?? new List<EventData>())
                    {
                        matchList.Add(new MatchInfo
                        {
                            League = league.LeagueName,
                            HomeTeam = match.HomeTeam,
                            AwayTeam = match.AwayTeam,
                            MatchTime = match.StartTime
                        });
                    }
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Server 1 data: {ex.Message}");
                return new List<MatchInfo>();
            }
        }

        private async Task<List<MatchInfo>> FetchServer2Data()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(Server2Url);

                var data = JsonSerializer.Deserialize<List<Server2LeagueData>>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var matchList = new List<MatchInfo>();
                foreach (var match in data ?? new List<Server2LeagueData>())
                {
                    matchList.Add(new MatchInfo
                    {
                        League = match.League,
                        HomeTeam = match.HomeTeam,
                        AwayTeam = match.AwayTeam,
                        MatchTime = match.MatchTime,
                        HomeScore = match.HomeScore,
                        AwayScore = match.AwayScore
                    });
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Server 2 data: {ex.Message}");
                return new List<MatchInfo>();
            }
        }

        private void PairMatches(object sender, RoutedEventArgs e)
        {
            if (Server1ListView.SelectedItem is MatchInfo match1 && Server2ListView.SelectedItem is MatchInfo match2)
            {
                var pairedMatch = new PairedMatchInfo
                {
                    Match1 = match1,
                    Match2 = match2
                };

                _pairedMatches.Add(pairedMatch);
                PairedMatchesListView.ItemsSource = null;
                PairedMatchesListView.ItemsSource = _pairedMatches;
            }
            else
            {
                MessageBox.Show("Please select one match from each list to pair.");
            }
        }

        private void BetOnMatch(object sender, RoutedEventArgs e)
        {
            if (PairedMatchesListView.SelectedItem is PairedMatchInfo pairedMatch)
            {
                _betMatches.Add(pairedMatch);
                BetMatchesListView.ItemsSource = null;
                BetMatchesListView.ItemsSource = _betMatches;
            }
            else
            {
                MessageBox.Show("Please select a paired match to bet on.");
            }
        }
    }

    public class MatchInfo
    {
        public string League { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string MatchTime { get; set; }
        public string HomeScore { get; set; }
        public string AwayScore { get; set; }

        public string DisplayInfo => $"{League}: {HomeTeam} {HomeScore} - {AwayScore} {AwayTeam}, Time: {MatchTime}";
    }

    public class PairedMatchInfo
    {
        public MatchInfo Match1 { get; set; }
        public MatchInfo Match2 { get; set; }

        public string DisplayPairInfo =>
            $"{Match1.League}: {Match1.HomeTeam} vs {Match1.AwayTeam} | {Match2.League}: {Match2.HomeTeam} vs {Match2.AwayTeam}";

        public string DisplayBetInfo => DisplayPairInfo;
    }

    public class LeagueData
    {
        [JsonPropertyName("league_name")]
        public string LeagueName { get; set; }

        [JsonPropertyName("events")]
        public List<EventData> Events { get; set; }
    }

    public class EventData
    {
        [JsonPropertyName("home_team")]
        public string HomeTeam { get; set; }

        [JsonPropertyName("away_team")]
        public string AwayTeam { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }
    }

    public class Server2LeagueData
    {
        [JsonPropertyName("league")]
        public string League { get; set; }

        [JsonPropertyName("home_team")]
        public string HomeTeam { get; set; }

        [JsonPropertyName("away_team")]
        public string AwayTeam { get; set; }

        [JsonPropertyName("match_time")]
        public string MatchTime { get; set; }

        [JsonPropertyName("home_score")]
        public string HomeScore { get; set; }

        [JsonPropertyName("away_score")]
        public string AwayScore { get; set; }
    }
}
