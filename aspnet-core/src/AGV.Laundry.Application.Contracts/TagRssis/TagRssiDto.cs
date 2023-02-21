using System;
using System.Collections.Generic;
using System.Text;

namespace AGV.Laundry.TagRssis
{
    public class TagMasterNodeDto
    {
        public string MasterNode { get; set; }
        public List<TagRssiDto> TagRssis { get; set; }
    }
    public class TagRssiDto
    {
        public string TagId { get; set; }
        public string BaseStationIP { get; set; }
        public int Rssi { get; set; }
        public float Battery { get; set; }

    }
}
