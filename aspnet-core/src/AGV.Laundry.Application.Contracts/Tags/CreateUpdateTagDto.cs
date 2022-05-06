using System;
using System.ComponentModel.DataAnnotations;

namespace AGV.Laundry.Tags
{
    public class CreateUpdateTagDto
    {
        [Required]
        [StringLength(128)]
        public string SystemName { get; set; }
        [Required]
        [StringLength(128)]
        public string DisplayName { get; set; }
        [Required]
        public bool Status { get; set; }
        public string TagId { get; set; }
        public string CartNo { get; set; }
    }
}
