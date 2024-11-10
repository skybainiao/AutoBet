using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Data;

namespace SportsBettingClient
{
    public partial class MainWindow : Window
    {
        private const string Server1Url = "http://127.0.0.1:5000/matches";

        public MainWindow()
        {
            InitializeComponent();
        }

        // 1号服务器数据加载
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

                if (data == null || data.Count == 0)
                {
                    MessageBox.Show("No leagues or matches received from Server 1.");
                    return new List<MatchInfo>();
                }

                var matchList = new List<MatchInfo>();
                foreach (var league in data)
                {
                    foreach (var match in league.Events)
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
                MessageBox.Show($"Error fetching data: {ex.Message}");
                return new List<MatchInfo>();
            }
        }
    }

    public class MatchInfo
    {
        public string League { get; set; } // 联赛名称
        public string HomeTeam { get; set; } // 主队名称
        public string AwayTeam { get; set; } // 客队名称
        public string MatchTime { get; set; } // 比赛时间

        public string DisplayInfo => $"{HomeTeam} vs {AwayTeam}, Time: {MatchTime}";
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
        [JsonPropertyName("event_id")]
        public int EventId { get; set; }

        [JsonPropertyName("home_team")]
        public string HomeTeam { get; set; }

        [JsonPropertyName("away_team")]
        public string AwayTeam { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("odds")]
        public List<OddsData> Odds { get; set; }
    }

    public class OddsData
    {
        public string BetType { get; set; }
        public int PeriodNumber { get; set; }
        public double Hdp { get; set; }
        public double HomeOdds { get; set; }
        public double AwayOdds { get; set; }
    }
}
