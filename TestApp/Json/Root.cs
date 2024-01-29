using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.json
{
    internal class Root
    {
        public Root() {
            Records = new List<Record>();
        }
        public List<Record> Records { get; set; }
    }
}
