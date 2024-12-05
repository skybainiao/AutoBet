using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoBet.Model
{
    public class MatchInfo : INotifyPropertyChanged
    {
        private bool _isBound;


        public string League { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string MatchTime { get; set; }
        public string HomeScore { get; set; }
        public string AwayScore { get; set; }

        // 新增的数据来源属性
        public int DataSource { get; set; }

        // 新增属性：IsBound
        public bool IsBound
        {
            get => _isBound;
            set
            {
                if (_isBound != value)
                {
                    _isBound = value;
                    OnPropertyChanged();
                }
            }
        }

        // 新增属性：DisplayScore
        public string DisplayScore => $"{HomeScore} - {AwayScore}";

        public string DisplayInfo => $"{League}: {HomeTeam} {HomeScore} - {AwayScore} {AwayTeam}, 时间: {MatchTime}";

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }




}
