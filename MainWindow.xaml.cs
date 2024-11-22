using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using AutoBet.DTO;
using AutoBet.Model;

namespace SportsBettingClient
{
    public partial class MainWindow : Window
    {
        private const string Server1Url = "http://localhost:8080/broadcast/matches/basic";
        private const string Server3Url = "http://localhost:8080/broadcast/matches2/basic";

        private readonly List<PairedMatchInfo> _pairedMatches = new();
        private readonly List<PairedMatchInfo> _betMatches = new();

        public MainWindow()
        {
            InitializeComponent();
            MessageBox.Show("MainWindow 已加载"); // 调试信息
        }

        // 加载服务器1的Match1数据
        private async void LoadServer1Data(object sender, RoutedEventArgs e)
        {
            var server1Data = await FetchServer1Data();
            if (server1Data != null)
            {
                MessageBox.Show($"加载了 {server1Data.Count} 条 Match1 数据"); // 调试信息
                var collectionView = new CollectionViewSource
                {
                    Source = server1Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Server1ListView.ItemsSource = collectionView.View;
            }
        }

        // 加载服务器3的Match2数据
        private async void LoadServer3Data(object sender, RoutedEventArgs e)
        {
            var server3Data = await FetchServer3Data();
            if (server3Data != null)
            {
                MessageBox.Show($"加载了 {server3Data.Count} 条 Match2 数据"); // 调试信息
                var collectionView = new CollectionViewSource
                {
                    Source = server3Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Server3ListView.ItemsSource = collectionView.View;
            }
        }

        // 获取服务器1的Match1数据
        private async Task<List<MatchInfo>> FetchServer1Data()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(Server1Url);

                var data = JsonSerializer.Deserialize<List<MatchBasicDTO>>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var matchList = new List<MatchInfo>();
                foreach (var match in data ?? new List<MatchBasicDTO>())
                {
                    matchList.Add(new MatchInfo
                    {
                        League = match.LeagueName,
                        HomeTeam = match.HomeTeam,
                        AwayTeam = match.AwayTeam,
                        MatchTime = match.MatchTime,
                        HomeScore = match.HomeScore?.ToString(),
                        AwayScore = match.AwayScore?.ToString()
                    });
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Server 1 data: {ex.Message}");
                return new List<MatchInfo>();
            }
        }

        // 获取服务器3的Match2数据
        private async Task<List<MatchInfo>> FetchServer3Data()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(Server3Url);

                var data = JsonSerializer.Deserialize<List<Match2BasicDTO>>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var matchList = new List<MatchInfo>();
                foreach (var dto in data ?? new List<Match2BasicDTO>())
                {
                    matchList.Add(new MatchInfo
                    {
                        League = dto.LeagueName,
                        HomeTeam = dto.HomeTeam,
                        AwayTeam = dto.AwayTeam,
                        MatchTime = dto.MatchTime,
                        HomeScore = dto.HomeScore?.ToString(),
                        AwayScore = dto.AwayScore?.ToString()
                    });
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Server 3 data: {ex.Message}");
                return new List<MatchInfo>();
            }
        }

        // 配对比赛
        private void PairMatches(object sender, RoutedEventArgs e)
        {
            // 配对Match1和Match2
            if (Server1ListView.SelectedItem is MatchInfo match1 && Server3ListView.SelectedItem is MatchInfo match2)
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
                MessageBox.Show("请从每个服务器中选择一场比赛进行配对。");
            }
        }

        // 投注比赛
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
                MessageBox.Show("请选中一场配对的比赛进行投注。");
            }
        }
    }

    // Models from the Models folder
    namespace AutoBet.Models
    {
        public class MatchInfo
        {
            public string League { get; set; }
            public string HomeTeam { get; set; }
            public string AwayTeam { get; set; }
            public string MatchTime { get; set; }
            public string HomeScore { get; set; }
            public string AwayScore { get; set; }

            public string DisplayInfo => $"{League}: {HomeTeam} {HomeScore} - {AwayScore} {AwayTeam}, 时间: {MatchTime}";
        }

        public class PairedMatchInfo
        {
            public MatchInfo Match1 { get; set; }
            public MatchInfo Match2 { get; set; }

            public string DisplayPairInfo =>
                $"{Match1.League}: {Match1.HomeTeam} vs {Match1.AwayTeam} | {Match2.League}: {Match2.HomeTeam} vs {Match2.AwayTeam}";

            public string DisplayBetInfo => DisplayPairInfo;
        }
    }
}
