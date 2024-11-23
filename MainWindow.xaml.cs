using AutoBet.DTO;
using AutoBet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace AutoBet
{
    public partial class MainWindow : Window
    {
        private const string Server1Url = "http://localhost:8080/broadcast/matches/basic";
        private const string Server3Url = "http://localhost:8080/broadcast/matches2/basic";

        private readonly List<PairedMatchInfo> _pairedMatches = new();

        // 定时器用于Match1和Match2的自动刷新
        private DispatcherTimer match1RefreshTimer;
        private DispatcherTimer match2RefreshTimer;

        public MainWindow()
        {
            InitializeComponent();
            // 初始化定时器
            match1RefreshTimer = new DispatcherTimer();
            match1RefreshTimer.Tick += Match1RefreshTimer_Tick;

            match2RefreshTimer = new DispatcherTimer();
            match2RefreshTimer.Tick += Match2RefreshTimer_Tick;
        }

        #region 刷新数据按钮点击事件

        // 刷新 Match1 数据（手动刷新）
        private async void RefreshMatch1Data(object sender, RoutedEventArgs e)
        {
            await LoadMatch1Data();
        }

        // 刷新 Match2 数据（手动刷新）
        private async void RefreshMatch2Data(object sender, RoutedEventArgs e)
        {
            await LoadMatch2Data();
        }

        #endregion

        #region 数据加载方法

        // 加载 Match1 数据
        private async Task LoadMatch1Data()
        {
            var match1Data = await FetchMatch1Data();
            if (match1Data != null)
            {
                // 设置 IsBound based on _pairedMatches
                foreach (var match in match1Data)
                {
                    if (_pairedMatches.Any(pm => MatchesAreEqual(pm.Match1, match)))
                    {
                        match.IsBound = true;
                    }
                }

                var collectionView = new CollectionViewSource
                {
                    Source = match1Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Match1ListView.ItemsSource = collectionView.View;
            }
        }

        // 加载 Match2 数据
        private async Task LoadMatch2Data()
        {
            var match2Data = await FetchMatch2Data();
            if (match2Data != null)
            {
                // 设置 IsBound based on _pairedMatches
                foreach (var match in match2Data)
                {
                    if (_pairedMatches.Any(pm => MatchesAreEqual(pm.Match2, match)))
                    {
                        match.IsBound = true;
                    }
                }

                var collectionView = new CollectionViewSource
                {
                    Source = match2Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Match2ListView.ItemsSource = collectionView.View;
            }
        }

        // 获取服务器1的Match1数据
        private async Task<List<MatchInfo>> FetchMatch1Data()
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
                        AwayScore = match.AwayScore?.ToString(),
                        IsBound = false // 初始化为未绑定
                    });
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Match1 data: {ex.Message}");
                return null;
            }
        }

        // 获取服务器3的Match2数据
        private async Task<List<MatchInfo>> FetchMatch2Data()
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
                        AwayScore = dto.AwayScore?.ToString(),
                        IsBound = false // 初始化为未绑定
                    });
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching Match2 data: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region ListView 选择变化事件

        // Match1 ListView 选择变化
        private void Match1ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateComparisonTexts();
        }

        // Match2 ListView 选择变化
        private void Match2ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateComparisonTexts();
        }

        #endregion

        #region 更新对比信息

        // 更新比赛对比信息的文本
        private void UpdateComparisonTexts()
        {
            if (Match1ListView.SelectedItem is MatchInfo selectedMatch1)
            {
                LeagueName1.Text = selectedMatch1.League;
                HomeTeam1.Text = selectedMatch1.HomeTeam;
                AwayTeam1.Text = selectedMatch1.AwayTeam;
            }
            else
            {
                LeagueName1.Text = "-";
                HomeTeam1.Text = "-";
                AwayTeam1.Text = "-";
            }

            if (Match2ListView.SelectedItem is MatchInfo selectedMatch2)
            {
                LeagueName2.Text = selectedMatch2.League;
                HomeTeam2.Text = selectedMatch2.HomeTeam;
                AwayTeam2.Text = selectedMatch2.AwayTeam;
            }
            else
            {
                LeagueName2.Text = "-";
                HomeTeam2.Text = "-";
                AwayTeam2.Text = "-";
            }
        }

        #endregion

        #region 添加绑定

        // 添加绑定按钮点击事件
        private void AddBinding(object sender, RoutedEventArgs e)
        {
            if (Match1ListView.SelectedItem is MatchInfo match1 && Match2ListView.SelectedItem is MatchInfo match2)
            {
                // 检查是否已经绑定
                if (_pairedMatches.Any(pm => (MatchesAreEqual(pm.Match1, match1) && MatchesAreEqual(pm.Match2, match2))))
                {
                    MessageBox.Show("这两场比赛已经绑定过了。");
                    return;
                }

                var pairedMatch = new PairedMatchInfo
                {
                    Match1 = match1,
                    Match2 = match2
                };

                _pairedMatches.Add(pairedMatch);
                BoundMatchesListView.ItemsSource = null;
                BoundMatchesListView.ItemsSource = _pairedMatches;

                // 标记比赛为已绑定
                match1.IsBound = true;
                match2.IsBound = true;

                MessageBox.Show("绑定成功！");
            }
            else
            {
                MessageBox.Show("请从 1网 和 2网 中各选择一场比赛进行绑定。");
            }
        }

        #endregion

        #region 刷新间隔选择事件

        // 显示 Match1 刷新间隔菜单
        private void ShowMatch1RefreshMenu(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();

            var intervals = new Dictionary<string, int>
            {
                { "1秒", 1000 },
                { "3秒", 3000 },
                { "5秒", 5000 },
                { "10秒", 10000 }
            };

            foreach (var item in intervals)
            {
                var menuItem = new MenuItem { Header = item.Key, Tag = item.Value };
                menuItem.Click += Match1RefreshInterval_Click;
                menu.Items.Add(menuItem);
            }

            menu.Items.Add(new Separator());

            var stopItem = new MenuItem { Header = "停止自动刷新" };
            stopItem.Click += Match1StopAutoRefresh_Click;
            menu.Items.Add(stopItem);

            menu.IsOpen = true;
        }

        // 显示 Match2 刷新间隔菜单
        private void ShowMatch2RefreshMenu(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();

            var intervals = new Dictionary<string, int>
            {
                { "1秒", 1000 },
                { "3秒", 3000 },
                { "5秒", 5000 },
                { "10秒", 10000 }
            };

            foreach (var item in intervals)
            {
                var menuItem = new MenuItem { Header = item.Key, Tag = item.Value };
                menuItem.Click += Match2RefreshInterval_Click;
                menu.Items.Add(menuItem);
            }

            menu.Items.Add(new Separator());

            var stopItem = new MenuItem { Header = "停止自动刷新" };
            stopItem.Click += Match2StopAutoRefresh_Click;
            menu.Items.Add(stopItem);

            menu.IsOpen = true;
        }

        // Match1 刷新间隔选择事件
        private void Match1RefreshInterval_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && int.TryParse(menuItem.Tag.ToString(), out int interval))
            {
                match1RefreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
                match1RefreshTimer.Start();
                MessageBox.Show($"1网数据将每 {interval / 1000} 秒刷新一次。");
            }
        }

        // Match1 停止自动刷新
        private void Match1StopAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            match1RefreshTimer.Stop();
            MessageBox.Show("已停止 1网 的自动刷新。");
        }

        // Match2 刷新间隔选择事件
        private void Match2RefreshInterval_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && int.TryParse(menuItem.Tag.ToString(), out int interval))
            {
                match2RefreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
                match2RefreshTimer.Start();
                MessageBox.Show($"2网数据将每 {interval / 1000} 秒刷新一次。");
            }
        }

        // Match2 停止自动刷新
        private void Match2StopAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            match2RefreshTimer.Stop();
            MessageBox.Show("已停止 2网 的自动刷新。");
        }

        // Match1 定时器 Tick 事件
        private async void Match1RefreshTimer_Tick(object sender, EventArgs e)
        {
            await LoadMatch1Data();
        }

        // Match2 定时器 Tick 事件
        private async void Match2RefreshTimer_Tick(object sender, EventArgs e)
        {
            await LoadMatch2Data();
        }

        #endregion

        #region 提交绑定数据

        // 提交绑定数据按钮点击事件
        private void SubmitBoundMatches_Click(object sender, RoutedEventArgs e)
        {
            // TODO: 实现将 _pairedMatches 发送到 Java 服务器的逻辑
            MessageBox.Show("提交按钮已点击。");
        }

        #endregion

        #region 删除绑定数据

        // 删除绑定数据按钮点击事件
        private void DeleteBoundMatch_Click(object sender, RoutedEventArgs e)
        {
            if (BoundMatchesListView.SelectedItem is PairedMatchInfo selectedPair)
            {
                // 从列表中移除
                _pairedMatches.Remove(selectedPair);
                BoundMatchesListView.ItemsSource = null;
                BoundMatchesListView.ItemsSource = _pairedMatches;

                // 更新 IsBound 状态
                selectedPair.Match1.IsBound = false;
                selectedPair.Match2.IsBound = false;

                MessageBox.Show("已删除选中的绑定比赛。");
            }
            else
            {
                MessageBox.Show("请先选择要删除的绑定比赛。");
            }
        }

        #endregion

        #region 辅助方法

        // 比较两个 MatchInfo 是否相同
        private bool MatchesAreEqual(MatchInfo a, MatchInfo b)
        {
            return a.League == b.League &&
                   a.HomeTeam == b.HomeTeam &&
                   a.AwayTeam == b.AwayTeam &&
                   a.MatchTime == b.MatchTime;
        }

        #endregion
    }
}
