using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBet.Model
{
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
}
