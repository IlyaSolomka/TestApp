using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TestApp.xml
{
    [XmlRoot(ElementName = "Card")]
    public class Card
    {

        [XmlElement(ElementName = "Pan")]
        public double Pan { get; set; }

        [XmlElement(ElementName = "ExpDate", Type = typeof(string))]
        public string ExpDate { get; set; }

        [XmlAttribute(AttributeName = "UserId")]
        public int UserId { get; set; }
    }
}
