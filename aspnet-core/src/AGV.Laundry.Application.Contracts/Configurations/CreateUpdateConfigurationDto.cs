using System;
using System.ComponentModel.DataAnnotations;

namespace AGV.Laundry.Configurations
{
    public class CreateUpdateConfigurationDto
    {
        [Required]
        public string Key { get; set; }
        [Required]
        public string Value { get; set; }
    }
}
