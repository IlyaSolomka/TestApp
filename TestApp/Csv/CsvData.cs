using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Csv
{
   internal class CsvData
    {
        public CsvData()
        {
            Rows = new List<CsvRow>();
        }
        public List<CsvRow> Rows { get; set; }
    }
}
