using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoBet.DTO
{
    public class BindingDTO
    {
        public string League1Name { get; set; }
        public string League2Name { get; set; }
        public string HomeTeam1Name { get; set; }
        public string HomeTeam2Name { get; set; }
        public string AwayTeam1Name { get; set; }
        public string AwayTeam2Name { get; set; }

        // 新增的数据来源字段
        public int DataSource1 { get; set; }
        public int DataSource2 { get; set; }
    }


}
