using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMEPDesignTool
{
    public class PlumbingFixture
    {
        public string Abbreviation { get; set; }
        public int Number { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public float Trap { get; set; }
        public float Waste { get; set; }
        public float Vent { get; set; }
        public float ColdWater { get; set; }
        public float HotWater { get; set; }
        public float FixtureDemand { get; set; }
        public float HotDemand { get; set; }
        public int DFU { get; set; }
    }
}
