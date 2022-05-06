using System;
using System.ComponentModel.DataAnnotations;

namespace AGV.Laundry.BaseStations
{
    public class CreateUpdateBaseStationDto
    {
        [Required]
        [StringLength(128)]
        public string SystemName { get; set; }
        [Required]
        [StringLength(128)]
        public string DisplayName { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public string BSIP { get; set; }
        [Required]
        public string Hotel { get; set; }
        [Required]
        public string LotNo { get; set; }
    }
}
