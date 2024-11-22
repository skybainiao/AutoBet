using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBet.Model
{
    public class PairedMatchInfo
    {
        public MatchInfo Match1 { get; set; }
        public MatchInfo Match2 { get; set; }

        public string DisplayPairInfo =>
            $"{Match1.League}: {Match1.HomeTeam} vs {Match1.AwayTeam} | {Match2.League}: {Match2.HomeTeam} vs {Match2.AwayTeam}";

        public string DisplayBetInfo => DisplayPairInfo;
    }
}
