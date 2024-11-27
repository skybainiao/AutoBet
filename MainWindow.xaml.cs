using AutoBet.DTO;
using AutoBet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Text;

namespace AutoBet
{
    public partial class MainWindow : Window
    {
        private const string Server1Url = "http://localhost:8080/broadcast/matches/basic";
        private const string Server2Url = "http://localhost:8080/broadcast/matches2/basic";
        private string selectedDataSource1 = null;
        private string selectedDataSource2 = null;


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

            // 加载绑定记录
            Loaded += async (s, e) => await LoadBindingRecords();
        }


        #region 刷新数据按钮点击事件

        // 刷新 Match1 数据（手动刷新）
        private async void RefreshMatch1Data(object sender, RoutedEventArgs e)
        {
            await LoadMatch1Data(0);
        }

        // 刷新 Match2 数据（手动刷新）
        private async void RefreshMatch2Data(object sender, RoutedEventArgs e)
        {
            await LoadMatch2Data(0);
        }

        #endregion

        #region 数据加载方法

        // 加载 Match1 数据
        private async Task LoadMatch1Data(int t)
        {
            if (string.IsNullOrEmpty(selectedDataSource1))
            {
                MessageBox.Show("请先在第一个数据框中选择数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 根据选择的数据源决定使用哪个服务器的URL
            string serverUrl = selectedDataSource1 == "1网滚球数据" ? Server1Url : Server2Url;

            var match1Data = await FetchMatch1Data(serverUrl);
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

                // 更新 ListView 数据源
                var collectionView = new CollectionViewSource
                {
                    Source = match1Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Match1ListView.ItemsSource = collectionView.View;

                if (t == 0)
                {
                    // 弹窗显示加载数据数量
                    if (match1Data.Count > 0)
                    {
                        MessageBox.Show($"A网滚球比赛加载完成，共加载 {match1Data.Count} 场比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("A网目前没有滚球比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else
            {
                if (t == 0)
                {
                    MessageBox.Show("A网数据加载失败。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // 加载 Match2 数据
        private async Task LoadMatch2Data(int t)
        {
            if (string.IsNullOrEmpty(selectedDataSource2))
            {
                MessageBox.Show("请先在第二个数据框中选择数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 根据选择的数据源决定使用哪个服务器的URL
            string serverUrl = selectedDataSource2 == "1网滚球数据" ? Server1Url : Server2Url;

            var match2Data = await FetchMatch2Data(serverUrl);
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

                // 更新 ListView 数据源
                var collectionView = new CollectionViewSource
                {
                    Source = match2Data
                };
                collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                Match2ListView.ItemsSource = collectionView.View;

                if (t == 0)
                {
                    // 弹窗显示加载数据数量
                    if (match2Data.Count > 0)
                    {
                        MessageBox.Show($"B网滚球比赛加载完成，共加载 {match2Data.Count} 场比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("B网目前没有滚球比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            else
            {
                if (t == 0)
                {
                    MessageBox.Show("B网数据加载失败。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        // 获取服务器1或2的 Match1 数据
        private async Task<List<MatchInfo>> FetchMatch1Data(string serverUrl)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(serverUrl);

                // 解析服务器返回的 JSON 数据
                var responseJson = JsonDocument.Parse(response);
                var matchCount = responseJson.RootElement.GetProperty("matchCount").GetInt32();
                var matches = responseJson.RootElement.GetProperty("matches").ToString();

                

                // 根据数据源解析不同的DTO
                List<MatchBasicDTO> data;
                if (serverUrl == Server1Url)
                {
                    data = JsonSerializer.Deserialize<List<MatchBasicDTO>>(matches, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }
                else
                {
                    data = JsonSerializer.Deserialize<List<Match2BasicDTO>>(matches, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                    .Select(dto => new MatchBasicDTO
                    {
                        // 映射 Match2BasicDTO 到 MatchBasicDTO
                        Id = dto.Id,
                        LeagueName = dto.LeagueName,
                        MatchTime = dto.MatchTime,
                        HomeTeam = dto.HomeTeam,
                        AwayTeam = dto.AwayTeam,
                        HomeScore = dto.HomeScore,
                        AwayScore = dto.AwayScore
                    })
                    .ToList();
                }

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



        // 获取服务器1或2的 Match2 数据
        private async Task<List<MatchInfo>> FetchMatch2Data(string serverUrl)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(serverUrl);

                // 解析服务器返回的 JSON 数据
                var responseJson = JsonDocument.Parse(response);
                var matchCount = responseJson.RootElement.GetProperty("matchCount").GetInt32();
                var matches = responseJson.RootElement.GetProperty("matches").ToString();

                 

                // 根据数据源解析不同的DTO
                List<Match2BasicDTO> data;
                if (serverUrl == Server2Url)
                {
                    data = JsonSerializer.Deserialize<List<Match2BasicDTO>>(matches, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                }
                else
                {
                    data = JsonSerializer.Deserialize<List<MatchBasicDTO>>(matches, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
                    .Select(dto => new Match2BasicDTO
                    {
                        // 映射 MatchBasicDTO 到 Match2BasicDTO
                        Id = dto.Id,
                        LeagueName = dto.LeagueName,
                        MatchTime = dto.MatchTime,
                        HomeTeam = dto.HomeTeam,
                        AwayTeam = dto.AwayTeam,
                        HomeScore = dto.HomeScore,
                        AwayScore = dto.AwayScore,
                       // 假设 MatchBasicDTO 没有 InsertedAt 字段，用 MatchTime 代替
                    })
                    .ToList();
                }

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
        // 显示数据源选择菜单 for 数据框1
        private void ShowDataSourceMenu1(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();

            var item1 = new MenuItem { Header = "1网滚球数据" };
            item1.Click += (s, args) =>
            {
                selectedDataSource1 = "1网滚球数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "1网滚球数据";
                }
                MessageBox.Show("已选择 1网滚球数据 为数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            menu.Items.Add(item1);

            var item2 = new MenuItem { Header = "2网滚球数据" };
            item2.Click += (s, args) =>
            {
                selectedDataSource1 = "2网滚球数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "2网滚球数据";
                }
                MessageBox.Show("已选择 2网滚球数据 为数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            menu.Items.Add(item2);

            menu.IsOpen = true;
        }

        // 显示数据源选择菜单 for 数据框2
        private void ShowDataSourceMenu2(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();

            var item1 = new MenuItem { Header = "1网滚球数据" };
            item1.Click += (s, args) =>
            {
                selectedDataSource2 = "1网滚球数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "1网滚球数据";
                }
                MessageBox.Show("已选择 1网滚球数据 为数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            menu.Items.Add(item1);

            var item2 = new MenuItem { Header = "2网滚球数据" };
            item2.Click += (s, args) =>
            {
                selectedDataSource2 = "2网滚球数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "2网滚球数据";
                }
                MessageBox.Show("已选择 2网滚球数据 为数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            };
            menu.Items.Add(item2);

            menu.IsOpen = true;
        }



        #region 添加绑定

        // 添加绑定按钮点击事件
        private void AddBinding(object sender, RoutedEventArgs e)
        {
            if (Match1ListView.SelectedItem is MatchInfo match1 && Match2ListView.SelectedItem is MatchInfo match2)
            {
                // 检查是否已经绑定
                if (_pairedMatches.Any(pm => MatchesAreEqual(pm.Match1, match1) && MatchesAreEqual(pm.Match2, match2)))
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
                MessageBox.Show("请从 A网 和 B网 中各选择一场比赛进行绑定。");
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
                MessageBox.Show($"A网数据将每 {interval / 1000} 秒刷新一次。");
            }
        }

        // Match1 停止自动刷新
        private void Match1StopAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            match1RefreshTimer.Stop();
            MessageBox.Show("已停止 A网 的自动刷新。");
        }

        // Match2 刷新间隔选择事件
        private void Match2RefreshInterval_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && int.TryParse(menuItem.Tag.ToString(), out int interval))
            {
                match2RefreshTimer.Interval = TimeSpan.FromMilliseconds(interval);
                match2RefreshTimer.Start();
                MessageBox.Show($"B网数据将每 {interval / 1000} 秒刷新一次。");
            }
        }

        // Match2 停止自动刷新
        private void Match2StopAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            match2RefreshTimer.Stop();
            MessageBox.Show("已停止 B网 的自动刷新。");
        }

        // Match1 定时器 Tick 事件
        private async void Match1RefreshTimer_Tick(object sender, EventArgs e)
        {
            await LoadMatch1Data(1);
        }

        // Match2 定时器 Tick 事件
        private async void Match2RefreshTimer_Tick(object sender, EventArgs e)
        {
            await LoadMatch2Data(1);
        }

        #endregion

        #region 提交绑定数据

        // 提交绑定数据按钮点击事件
        private async void SubmitBoundMatches_Click(object sender, RoutedEventArgs e)
        {
            if (!_pairedMatches.Any())
            {
                MessageBox.Show("没有绑定的数据可提交。");
                return;
            }

            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:8080/api/"); // Java服务器地址

                // 构造 BindingDTO 列表
                var bindingDtos = _pairedMatches.Select(pm => new BindingDTO
                {
                    League1Name = pm.Match1.League,
                    League2Name = pm.Match2.League,
                    HomeTeam1Name = pm.Match1.HomeTeam,
                    HomeTeam2Name = pm.Match2.HomeTeam,
                    AwayTeam1Name = pm.Match1.AwayTeam,
                    AwayTeam2Name = pm.Match2.AwayTeam
                }).ToList();

                var json = JsonSerializer.Serialize(bindingDtos, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("bindings", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"提交绑定成功！\n{resultMessage}");

                    // 清空本地绑定列表并刷新已绑定比赛列表
                    _pairedMatches.Clear();
                    BoundMatchesListView.ItemsSource = null;
                    BoundMatchesListView.ItemsSource = _pairedMatches;

                    // 刷新绑定记录页面
                    await LoadBindingRecords();
                }
                else
                {
                    MessageBox.Show($"提交绑定失败: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"提交绑定时发生错误: {ex.Message}");
            }
        }







        private async Task LoadBindingRecords()
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:8080/api/"); // Java服务器地址

                var response = await client.GetAsync("bindings");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var bindings = JsonSerializer.Deserialize<List<BindingRecordDTO>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    BindingRecordsListView.ItemsSource = bindings;
                }
                else
                {
                    MessageBox.Show($"获取绑定记录失败: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取绑定记录时发生错误: {ex.Message}");
            }
        }



        private async void DeleteBindingRecord_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button deleteButton && deleteButton.Tag is long bindingId)
            {
                var confirmResult = MessageBox.Show("您确定要删除选中的绑定记录吗？", "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmResult != MessageBoxResult.Yes)
                    return;

                try
                {
                    using var client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:8080/api/"); // Java服务器地址

                    var response = await client.DeleteAsync($"bindings/{bindingId}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("已成功删除绑定记录。");

                        // 刷新绑定记录页面
                        await LoadBindingRecords();

                        // 更新本地绑定列表，取消绑定状态
                        // 获取已删除的 BindingRecordDTO
                        var deletedBinding = ((List<BindingRecordDTO>)BindingRecordsListView.ItemsSource)
                                              .FirstOrDefault(b => b.Id == bindingId);

                        if (deletedBinding != null)
                        {
                            var bindingToRemove = _pairedMatches.FirstOrDefault(pm =>
                                pm.Match1.League == deletedBinding.League1Name &&
                                pm.Match2.League == deletedBinding.League2Name &&
                                pm.Match1.HomeTeam == deletedBinding.HomeTeam1Name &&
                                pm.Match2.HomeTeam == deletedBinding.HomeTeam2Name &&
                                pm.Match1.AwayTeam == deletedBinding.AwayTeam1Name &&
                                pm.Match2.AwayTeam == deletedBinding.AwayTeam2Name
                            );

                            if (bindingToRemove != null)
                            {
                                bindingToRemove.Match1.IsBound = false;
                                bindingToRemove.Match2.IsBound = false;
                                _pairedMatches.Remove(bindingToRemove);
                                BoundMatchesListView.ItemsSource = null;
                                BoundMatchesListView.ItemsSource = _pairedMatches;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"删除绑定记录失败: {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除绑定记录时发生错误: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("请先选择要删除的绑定记录。");
            }
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

            }
            else
            {
                MessageBox.Show("请先选择要删除的绑定比赛。");
            }
        }

        #endregion

        #region 自动绑定

        // 自动绑定按钮点击事件
        private void AutoBind_Click(object sender, RoutedEventArgs e)
        {
            // 获取所有未绑定的 Match1 和 Match2
            var availableMatch1 = LoadAllMatch1Data();
            var availableMatch2 = LoadAllMatch2Data();

            var autoBoundMatches = new List<PairedMatchInfo>();

            foreach (var m1 in availableMatch1)
            {
                foreach (var m2 in availableMatch2)
                {
                    if (!_pairedMatches.Any(pm => MatchesAreEqual(pm.Match1, m1) && MatchesAreEqual(pm.Match2, m2)))
                    {
                        if (AreMatchesSimilar(m1, m2))
                        {
                            autoBoundMatches.Add(new PairedMatchInfo { Match1 = m1, Match2 = m2 });
                        }
                    }
                }
            }

            if (autoBoundMatches.Any())
            {
                foreach (var pair in autoBoundMatches)
                {
                    _pairedMatches.Add(pair);
                    pair.Match1.IsBound = true;
                    pair.Match2.IsBound = true;
                }

                BoundMatchesListView.ItemsSource = null;
                BoundMatchesListView.ItemsSource = _pairedMatches;

                MessageBox.Show($"已自动绑定 {autoBoundMatches.Count} 场比赛。");
            }
            else
            {
                MessageBox.Show("没有符合自动绑定条件的比赛。");
            }
        }

        // 加载所有 Match1 数据（包括已绑定和未绑定）
        private List<MatchInfo> LoadAllMatch1Data()
        {
            var currentView = Match1ListView.ItemsSource as CollectionView;
            if (currentView != null)
            {
                return currentView.Cast<MatchInfo>().ToList();
            }
            return new List<MatchInfo>();
        }

        // 加载所有 Match2 数据（包括已绑定和未绑定）
        private List<MatchInfo> LoadAllMatch2Data()
        {
            var currentView = Match2ListView.ItemsSource as CollectionView;
            if (currentView != null)
            {
                return currentView.Cast<MatchInfo>().ToList();
            }
            return new List<MatchInfo>();
        }

        // 判断两个比赛是否相似
        private bool AreMatchesSimilar(MatchInfo m1, MatchInfo m2)
        {
            // 使用简单的词汇匹配逻辑
            // 可以根据需求调整匹配算法，例如使用Levenshtein距离或其他文本相似性算法
            bool leagueMatch = WordsContainMatch(m1.League, m2.League);
            bool homeTeamMatch = WordsContainMatch(m1.HomeTeam, m2.HomeTeam);
            bool awayTeamMatch = WordsContainMatch(m1.AwayTeam, m2.AwayTeam);

            // 定义相似的条件：联赛、主队和客队都有部分匹配
            return leagueMatch && homeTeamMatch && awayTeamMatch;
        }

        // 检查两个字符串是否有共同的单词
        private bool WordsContainMatch(string str1, string str2)
        {
            var words1 = Regex.Split(str1.ToLower(), @"\W+").Where(w => w.Length > 0).ToList();
            var words2 = Regex.Split(str2.ToLower(), @"\W+").Where(w => w.Length > 0).ToList();

            return words1.Intersect(words2).Any();
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
