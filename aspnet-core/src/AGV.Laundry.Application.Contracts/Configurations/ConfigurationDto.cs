using System;
using Volo.Abp.Application.Dtos;

namespace AGV.Laundry.Configurations
{
    public class ConfigurationDto : AuditedEntityDto<Guid>
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}