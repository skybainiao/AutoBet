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
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;


namespace AutoBet
{
    public partial class MainWindow : Window
    {
        private const string Server1Url = "http://154.86.116.11:5000/today_fixtures";
        private const string Server2Url = "http://160.25.20.81:5001/matches";
        private string selectedDataSource1 = null;
        private string selectedDataSource2 = null;
        // 在 MainWindow 类内添加以下字段
        private List<DTO.BindingRecordDTO> _bindings = new List<DTO.BindingRecordDTO>();
        // 定义默认的相似度阈值
        private double _precisionThreshold = 0.85;


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


        // 刷新 Match1 数据（手动刷新）
        private async void RefreshMatch1Data(object sender, RoutedEventArgs e)
        {
            var refreshButton = sender as Button;
            refreshButton.IsEnabled = false; // 禁用按钮防止重复点击

            try
            {
                // 显示加载覆盖层
                LoadingOverlay.Visibility = Visibility.Visible;

                await LoadBindingRecords(); // 先加载绑定记录
                await LoadMatch1Data(0); // 然后加载 Match1 数据
            }
            catch (Exception ex)
            {
                // 异常处理（已经在 LoadMatch1Data 中处理了）
            }
            finally
            {
                // 隐藏加载覆盖层
                LoadingOverlay.Visibility = Visibility.Collapsed;
                refreshButton.IsEnabled = true; // 重新启用按钮
            }
        }

        // 刷新 Match2 数据（手动刷新）
        private async void RefreshMatch2Data(object sender, RoutedEventArgs e)
        {
            var refreshButton = sender as Button;
            refreshButton.IsEnabled = false; // 禁用按钮防止重复点击

            try
            {
                // 显示加载覆盖层
                LoadingOverlay.Visibility = Visibility.Visible;

                await LoadBindingRecords(); // 先加载绑定记录
                await LoadMatch2Data(0); // 然后加载 Match2 数据
            }
            catch (Exception ex)
            {
                // 异常处理（已经在 LoadMatch2Data 中处理了）
            }
            finally
            {
                // 隐藏加载覆盖层
                LoadingOverlay.Visibility = Visibility.Collapsed;
                refreshButton.IsEnabled = true; // 重新启用按钮
            }
        }



        #region 数据加载方法

        // 加载 Match1 数据
        private async Task LoadMatch1Data(int t)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedDataSource1))
                {
                    MessageBox.Show("请先在第一个数据框中选择数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string serverUrl = selectedDataSource1 == "1网今日数据" ? Server1Url : Server2Url;

                var match1Data = await FetchMatch1Data(serverUrl);
                if (match1Data != null)
                {
                    // 过滤已绑定的比赛
                    var filteredMatch1Data = match1Data.Where(match => !IsMatchBound(match, selectedDataSource1)).ToList();

                    // 更新 ListView 数据源
                    var collectionView = new CollectionViewSource
                    {
                        Source = filteredMatch1Data
                    };
                    collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                    Match1ListView.ItemsSource = collectionView.View;

                    if (t == 0)
                    {
                        // 弹窗显示加载数据数量
                        if (filteredMatch1Data.Count > 0)
                        {
                            MessageBox.Show($"A网今日比赛加载完成，共加载 {filteredMatch1Data.Count} 场比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("A网目前没有今日比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            catch (Exception ex)
            {
                MessageBox.Show($"加载 A网 数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // 加载 Match2 数据
        private async Task LoadMatch2Data(int t)
        {
            try
            {
                if (string.IsNullOrEmpty(selectedDataSource2))
                {
                    MessageBox.Show("请先在第二个数据框中选择数据源。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string serverUrl = selectedDataSource2 == "1网今日数据" ? Server1Url : Server2Url;

                var match2Data = await FetchMatch2Data(serverUrl);
                if (match2Data != null)
                {
                    // 过滤已绑定的比赛
                    var filteredMatch2Data = match2Data.Where(match => !IsMatchBound(match, selectedDataSource2)).ToList();

                    // 更新 ListView 数据源
                    var collectionView = new CollectionViewSource
                    {
                        Source = filteredMatch2Data
                    };
                    collectionView.GroupDescriptions.Add(new PropertyGroupDescription("League"));
                    Match2ListView.ItemsSource = collectionView.View;

                    if (t == 0)
                    {
                        // 弹窗显示加载数据数量
                        if (filteredMatch2Data.Count > 0)
                        {
                            MessageBox.Show($"B网今日比赛加载完成，共加载 {filteredMatch2Data.Count} 场比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("B网目前没有今日比赛。", "加载完成", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            catch (Exception ex)
            {
                MessageBox.Show($"加载 B网 数据失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private bool IsMatchBound(Model.MatchInfo match, string selectedDataSource)
        {
            if (selectedDataSource == "1网今日数据")
            {
                return _bindings.Any(binding =>
                    string.Equals(binding.League1Name, match.League, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(binding.HomeTeam1Name, match.HomeTeam, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(binding.AwayTeam1Name, match.AwayTeam, StringComparison.OrdinalIgnoreCase)
                );
            }
            else if (selectedDataSource == "2网今日数据")
            {
                return _bindings.Any(binding =>
                    string.Equals(binding.League2Name, match.League, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(binding.HomeTeam2Name, match.HomeTeam, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(binding.AwayTeam2Name, match.AwayTeam, StringComparison.OrdinalIgnoreCase)
                );
            }

            return false;
        }


        // 获取服务器1或2的 Match1 数据
        private async Task<List<MatchInfo>> FetchMatch1Data(string serverUrl)
        {
            try
            {
                using var client = new HttpClient();
                var response = await client.GetStringAsync(serverUrl);

                var responseJson = JsonDocument.Parse(response);
                var matchCount = responseJson.RootElement.GetProperty("count").GetInt32();
                var fixtures = responseJson.RootElement.GetProperty("fixtures");
                var dataSource = responseJson.RootElement.GetProperty("dataSource").GetInt32();

                var matchList = new List<MatchInfo>();
                foreach (var match in fixtures.EnumerateArray())
                {
                    matchList.Add(new MatchInfo
                    {
                        League = match.GetProperty("league").GetString(),
                        HomeTeam = match.GetProperty("home_team").GetString(),
                        AwayTeam = match.GetProperty("away_team").GetString(),
                        MatchTime = match.GetProperty("time").GetString(), // 读取 time 字段
                        IsBound = false,
                        DataSource = dataSource // 设置数据来源
                    });
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error fetching Match1 data: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

                var responseJson = JsonDocument.Parse(response);
                var matchCount = responseJson.RootElement.GetProperty("count").GetInt32();
                var fixtures = responseJson.RootElement.GetProperty("fixtures");
                var dataSource = responseJson.RootElement.GetProperty("dataSource").GetInt32();

                var matchList = new List<MatchInfo>();
                foreach (var match in fixtures.EnumerateArray())
                {
                    matchList.Add(new MatchInfo
                    {
                        League = match.GetProperty("league").GetString(),
                        HomeTeam = match.GetProperty("home_team").GetString(),
                        AwayTeam = match.GetProperty("away_team").GetString(),
                        MatchTime = match.GetProperty("time").GetString(), // 读取 time 字段
                        IsBound = false,
                        DataSource = dataSource // 设置数据来源
                    });
                }

                return matchList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Error fetching Match2 data: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }





        #endregion
        // 显示数据源选择菜单 for 数据框1
        private void ShowDataSourceMenu1(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();

            var item1 = new MenuItem { Header = "1网今日数据" };
            item1.Click += (s, args) =>
            {
                selectedDataSource1 = "1网今日数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "1网今日数据";
                }
                
            };
            menu.Items.Add(item1);

            var item2 = new MenuItem { Header = "2网今日数据" };
            item2.Click += (s, args) =>
            {
                selectedDataSource1 = "2网今日数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "2网今日数据";
                }
                 
            };
            menu.Items.Add(item2);

            menu.IsOpen = true;
        }

        // 显示数据源选择菜单 for 数据框2
        private void ShowDataSourceMenu2(object sender, RoutedEventArgs e)
        {
            var menu = new ContextMenu();

            var item1 = new MenuItem { Header = "1网今日数据" };
            item1.Click += (s, args) =>
            {
                selectedDataSource2 = "1网今日数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "1网今日数据";
                }
                
            };
            menu.Items.Add(item1);

            var item2 = new MenuItem { Header = "2网今日数据" };
            item2.Click += (s, args) =>
            {
                selectedDataSource2 = "2网今日数据";
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "2网今日数据";
                }
                 
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
                { "3秒", 3000 },
                { "5秒", 5000 },
                { "10秒", 10000 },
                { "20秒", 20000 }
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
                { "3秒", 3000 },
                { "5秒", 5000 },
                { "10秒", 10000 },
                { "20秒", 20000 }
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
                MessageBox.Show(this, "没有绑定的数据可提交。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var submitButton = sender as Button;
            submitButton.IsEnabled = false; // 禁用按钮防止重复点击

            try
            {
                // 显示加载覆盖层
                LoadingOverlay.Visibility = Visibility.Visible;

                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://160.25.20.123:8080/api/"); // Java服务器地址

                // 构造 BindingDTO 列表，并设置数据来源
                var bindingDtos = _pairedMatches.Select(pm => new BindingDTO
                {
                    League1Name = pm.Match1.League,
                    League2Name = pm.Match2.League,
                    HomeTeam1Name = pm.Match1.HomeTeam,
                    HomeTeam2Name = pm.Match2.HomeTeam,
                    AwayTeam1Name = pm.Match1.AwayTeam,
                    AwayTeam2Name = pm.Match2.AwayTeam,
                    DataSource1 = pm.Match1.DataSource, // 设置 DataSource1
                    DataSource2 = pm.Match2.DataSource  // 设置 DataSource2
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
                    MessageBox.Show(this, $"提交绑定成功！\n{resultMessage}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    RemoveBoundMatchesFromListViews();
                    // 清空绑定列表并刷新已绑定比赛列表
                    _pairedMatches.Clear();
                    BoundMatchesListView.ItemsSource = null;
                    BoundMatchesListView.ItemsSource = _pairedMatches;

                    // 刷新绑定记录页面
                    await LoadBindingRecords();
                }
                else
                {
                    MessageBox.Show(this, $"提交绑定失败: {response.ReasonPhrase}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"提交绑定时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 隐藏加载覆盖层
                LoadingOverlay.Visibility = Visibility.Collapsed;
                submitButton.IsEnabled = true; // 重新启用按钮
            }
        }




        private void RemoveBoundMatchesFromListViews()
        {
            // 创建绑定比赛的副本，以避免在遍历时修改集合
            var boundMatches = _pairedMatches.ToList();

            // 移除 Match1ListView 中的绑定比赛
            var currentMatch1Items = Match1ListView.Items.OfType<MatchInfo>().ToList();
            foreach (var pairedMatch in boundMatches)
            {
                var match1ToRemove = currentMatch1Items.FirstOrDefault(m =>
                    m.League == pairedMatch.Match1.League &&
                    m.HomeTeam == pairedMatch.Match1.HomeTeam &&
                    m.AwayTeam == pairedMatch.Match1.AwayTeam &&
                    m.MatchTime == pairedMatch.Match1.MatchTime);

                if (match1ToRemove != null)
                {
                    currentMatch1Items.Remove(match1ToRemove);
                }
            }

            // 重新创建 CollectionViewSource 并设置 ItemsSource
            var collectionView1 = new CollectionViewSource { Source = currentMatch1Items };
            collectionView1.GroupDescriptions.Add(new PropertyGroupDescription("League"));
            Match1ListView.ItemsSource = collectionView1.View;

            // 移除 Match2ListView 中的绑定比赛
            var currentMatch2Items = Match2ListView.Items.OfType<MatchInfo>().ToList();
            foreach (var pairedMatch in boundMatches)
            {
                var match2ToRemove = currentMatch2Items.FirstOrDefault(m =>
                    m.League == pairedMatch.Match2.League &&
                    m.HomeTeam == pairedMatch.Match2.HomeTeam &&
                    m.AwayTeam == pairedMatch.Match2.AwayTeam &&
                    m.MatchTime == pairedMatch.Match2.MatchTime);

                if (match2ToRemove != null)
                {
                    currentMatch2Items.Remove(match2ToRemove);
                }
            }

            // 重新创建 CollectionViewSource 并设置 ItemsSource
            var collectionView2 = new CollectionViewSource { Source = currentMatch2Items };
            collectionView2.GroupDescriptions.Add(new PropertyGroupDescription("League"));
            Match2ListView.ItemsSource = collectionView2.View;
        }



        private async Task LoadBindingRecords()
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri("http://160.25.20.123:8080/api/"); // API 基础地址

                var response = await client.GetAsync("bindings"); // 假设 API 路径为 /bindings

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var bindings = JsonSerializer.Deserialize<List<DTO.BindingRecordDTO>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (bindings != null)
                    {
                        _bindings = bindings; // 填充私有字段

                        // 按 DataSource1 和 DataSource2 分组
                        var groupedBindings = bindings
                            .GroupBy(b => new { b.DataSource1, b.DataSource2 })
                            .Select(g => new Model.BindingRecordGroup
                            {
                                DataSource1 = g.Key.DataSource1,
                                DataSource2 = g.Key.DataSource2,
                                Leagues = g.GroupBy(b => new { b.League1Name, b.League2Name })
                                           .Select(lg => new Model.LeagueBinding
                                           {
                                               League1Name = lg.Key.League1Name,
                                               League2Name = lg.Key.League2Name,
                                               Bindings = lg.ToList()
                                           })
                                           .ToList()
                            })
                            .ToList();

                        BindingRecordsTreeView.ItemsSource = groupedBindings;
                    }
                }
                else
                {
                    MessageBox.Show($"获取绑定记录失败: {response.ReasonPhrase}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"获取绑定记录时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }








        private async void DeleteBindingRecord_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem deleteMenuItem && deleteMenuItem.Tag != null)
            {
                if (long.TryParse(deleteMenuItem.Tag.ToString(), out long bindingId))
                {
                    var confirmResult = MessageBox.Show(
                        "确定要删除这条绑定记录吗？",
                        "确认删除",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

                    if (confirmResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            using var client = new HttpClient();
                            client.BaseAddress = new Uri("http://160.25.20.123:8080/api/");

                            var response = await client.DeleteAsync($"bindings/{bindingId}");

                            if (response.IsSuccessStatusCode)
                            {
                                MessageBox.Show("绑定记录已成功删除。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                                // 重新加载绑定记录
                                await LoadBindingRecords();
                            }
                            else
                            {
                                MessageBox.Show($"删除绑定记录失败: {response.ReasonPhrase}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"删除绑定记录时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("无效的绑定记录ID。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("未能识别要删除的绑定记录。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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


        private void DeleteAllBoundMatches_Click(object sender, RoutedEventArgs e)
        {
            if (!_pairedMatches.Any())
            {
                MessageBox.Show("当前没有绑定的比赛可以移除。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmResult = MessageBox.Show(
                "确定要移除所有绑定内容吗？此操作不可撤销。",
                "确认清空",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirmResult == MessageBoxResult.Yes)
            {
                // 遍历所有绑定内容，更新 IsBound 状态
                foreach (var pair in _pairedMatches)
                {
                    pair.Match1.IsBound = false;
                    pair.Match2.IsBound = false;
                }

                // 清空绑定列表
                _pairedMatches.Clear();
                BoundMatchesListView.ItemsSource = null;

                
            }
             
        }





        #region 自动绑定

        // 自动绑定按钮点击事件
        private async void AutoBind_Click(object sender, RoutedEventArgs e)
        {
            var autoBindButton = sender as Button;
            autoBindButton.IsEnabled = false; // 禁用按钮防止重复点击

            try
            {
                // 获取当前相似度阈值
                double similarityThreshold = PrecisionSlider.Value;

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
                            // 使用改进的相似性匹配逻辑
                            if (AreMatchesSimilar(m1, m2, similarityThreshold))
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

                    MessageBox.Show(this, $"已自动绑定 {autoBoundMatches.Count} 场比赛。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(this, "没有符合自动绑定条件的比赛。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"自动绑定时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                autoBindButton.IsEnabled = true; // 重新启用按钮
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
        private bool AreMatchesSimilar(MatchInfo m1, MatchInfo m2, double similarityThreshold)
        {
            // 计算联赛、主队和客队的相似度评分
            double leagueSimilarity = CalculateStringSimilarity(m1.League, m2.League);
            double homeTeamSimilarity = CalculateStringSimilarity(m1.HomeTeam, m2.HomeTeam);
            double awayTeamSimilarity = CalculateStringSimilarity(m1.AwayTeam, m2.AwayTeam);

            // 如果所有评分都超过当前阈值，则认为两场比赛相似
            return leagueSimilarity >= similarityThreshold &&
                   homeTeamSimilarity >= similarityThreshold &&
                   awayTeamSimilarity >= similarityThreshold;
        }


        // 计算两个字符串的相似性（使用 Jaccard 相似性算法）
        private double CalculateStringSimilarity(string str1, string str2)
        {
            if (string.IsNullOrWhiteSpace(str1) || string.IsNullOrWhiteSpace(str2))
            {
                return 0.0;
            }

            var set1 = new HashSet<string>(Regex.Split(str1.ToLower(), @"\W+").Where(w => w.Length > 0));
            var set2 = new HashSet<string>(Regex.Split(str2.ToLower(), @"\W+").Where(w => w.Length > 0));

            int intersectionCount = set1.Intersect(set2).Count();
            int unionCount = set1.Union(set2).Count();

            return unionCount == 0 ? 0.0 : (double)intersectionCount / unionCount;
        }

        #endregion


        // 滑块值变化事件处理程序
        private void PrecisionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PrecisionValueText != null)
            {
                PrecisionValueText.Text = e.NewValue.ToString("F2");
            }
        }





        private async void AutoBind_Click1(object sender, RoutedEventArgs e)
        {
            var autoBindButton = sender as Button;
            autoBindButton.IsEnabled = false; // 禁用按钮防止重复点击

            try
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
                            if (AreMatchesSimilar1(m1, m2))
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

                    MessageBox.Show(this, $"已自动绑定 {autoBoundMatches.Count} 场比赛。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(this, "没有符合自动绑定条件的比赛。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"自动绑定时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {

                autoBindButton.IsEnabled = true; // 重新启用按钮
            }
        }// 判断两个比赛是否相似
        private bool AreMatchesSimilar1(MatchInfo m1, MatchInfo m2)
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

        #region 辅助方法

        // 比较两个 MatchInfo 是否相同
        private bool MatchesAreEqual(MatchInfo a, MatchInfo b)
        {
            return a.League == b.League &&
                   a.HomeTeam == b.HomeTeam &&
                   a.AwayTeam == b.AwayTeam;
                   
        }

        #endregion






        // 复制 Match1ListView 的菜单项点击事件处理程序
        private void CopyMatch1MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedMatch = Match1ListView.SelectedItem as Model.MatchInfo;
            if (selectedMatch != null)
            {
                string copyText = $"联赛: {selectedMatch.League}\t主队: {selectedMatch.HomeTeam}\t客队: {selectedMatch.AwayTeam}\t比赛时间: {selectedMatch.MatchTime}";
                Clipboard.SetDataObject(copyText);
                MessageBox.Show("A网比赛信息已复制到剪贴板。", "复制成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("请先选择一场比赛。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // 复制 Match2ListView 的菜单项点击事件处理程序
        private void CopyMatch2MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedMatch = Match2ListView.SelectedItem as Model.MatchInfo;
            if (selectedMatch != null)
            {
                string copyText = $"联赛: {selectedMatch.League}\t主队: {selectedMatch.HomeTeam}\t客队: {selectedMatch.AwayTeam}\t比赛时间: {selectedMatch.MatchTime}";
                Clipboard.SetDataObject(copyText);
                MessageBox.Show("B网比赛信息已复制到剪贴板。", "复制成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("请先选择一场比赛。", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        /// <summary>
        /// 根据搜索关键词过滤 Match1ListView 的显示内容。
        /// </summary>
        private void FilterMatch1List(string searchText)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Match1ListView.ItemsSource);
            if (view != null)
            {
                view.Filter = item =>
                {
                    var match = item as Model.MatchInfo;
                    if (match == null) return false;

                    if (string.IsNullOrWhiteSpace(searchText))
                        return true;

                    searchText = searchText.ToLower();

                    return (match.MatchTime != null && match.MatchTime.ToLower().Contains(searchText)) ||
                           (match.League != null && match.League.ToLower().Contains(searchText)) ||
                           (match.HomeTeam != null && match.HomeTeam.ToLower().Contains(searchText)) ||
                           (match.AwayTeam != null && match.AwayTeam.ToLower().Contains(searchText));
                };
            }
        }

        /// <summary>
        /// 根据搜索关键词过滤 Match2ListView 的显示内容。
        /// </summary>
        private void FilterMatch2List(string searchText)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Match2ListView.ItemsSource);
            if (view != null)
            {
                view.Filter = item =>
                {
                    var match = item as Model.MatchInfo;
                    if (match == null) return false;

                    if (string.IsNullOrWhiteSpace(searchText))
                        return true;

                    searchText = searchText.ToLower();

                    return (match.MatchTime != null && match.MatchTime.ToLower().Contains(searchText)) ||
                           (match.League != null && match.League.ToLower().Contains(searchText)) ||
                           (match.HomeTeam != null && match.HomeTeam.ToLower().Contains(searchText)) ||
                           (match.AwayTeam != null && match.AwayTeam.ToLower().Contains(searchText));
                };
            }
        }




        // 搜索框1的文本变化事件处理程序
        private void Match1SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox != null)
            {
                string searchText = searchBox.Text;
                FilterMatch1List(searchText);
            }
        }

        // 搜索框2的文本变化事件处理程序
        private void Match2SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBox = sender as TextBox;
            if (searchBox != null)
            {
                string searchText = searchBox.Text;
                FilterMatch2List(searchText);
            }
        }










    }








}
