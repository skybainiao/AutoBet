using AutoBet.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;

namespace AutoBet.Model
{
    public class BindingRecordGroup
    {
        public int DataSource1 { get; set; }
        public int DataSource2 { get; set; }
        public string DataSourceName => $"{GetDataSourceName(DataSource1)} ↔ {GetDataSourceName(DataSource2)}";
        public List<LeagueBinding> Leagues { get; set; }

        private string GetDataSourceName(int dataSource)
        {
            return dataSource switch
            {
                1 => "1网",
                2 => "2网",
                _ => $"网{dataSource}"
            };
        }
    }

    public class LeagueBinding
    {
        public string League1Name { get; set; }
        public string League2Name { get; set; }
        public string LeagueBindingName => $"{League1Name} ↔ {League2Name}";
        public List<DTO.BindingRecordDTO> Bindings { get; set; }
    }
}