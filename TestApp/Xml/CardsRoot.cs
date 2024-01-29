using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TestApp.xml
{
    [XmlRoot(ElementName = "Cards")]
    public class CardsRoot
    {
        [XmlElement(ElementName = "Card")]
        public List<Card> Cards { get; set; }
    }
}
