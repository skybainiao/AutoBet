using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBet.Model
{
    public class Match2
    {
        public long Id { get; set; }
        public string LeagueName { get; set; }
        public string MatchTime { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public int? HomeScore { get; set; }
        public int? AwayScore { get; set; }
        public Dictionary<string, string> Odds { get; set; }
        public DateTime InsertedAt { get; set; }
    }
}
