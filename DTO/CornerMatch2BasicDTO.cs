using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutoBet.DTO
{
    public class CornerMatch2BasicDTO
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("leagueName")]
        public string LeagueName { get; set; }

        [JsonPropertyName("matchTime")]
        public string MatchTime { get; set; }

        [JsonPropertyName("homeTeam")]
        public string HomeTeam { get; set; }

        [JsonPropertyName("awayTeam")]
        public string AwayTeam { get; set; }

        [JsonPropertyName("homeScore")]
        public int? HomeScore { get; set; }

        [JsonPropertyName("awayScore")]
        public int? AwayScore { get; set; }

        [JsonPropertyName("insertedAt")]
        public DateTime InsertedAt { get; set; }
    }
}
