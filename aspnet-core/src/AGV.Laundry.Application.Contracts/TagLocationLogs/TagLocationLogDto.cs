using System;
using AGV.Laundry.Enums;
using Volo.Abp.Application.Dtos;

namespace AGV.Laundry.TagLocationLogs
{
    public class TagLocationLogDto
    {
        public Guid BasestationId { get; set; }
        public string BasestationIp { get; set; }
        public string Hotel { get; set; }
        public string Lot { get; set; }
        public Guid TagId { get; set; }
        public string TagMac { get; set; }
        public string Cart { get; set; }
        public TagLocationLogStatus Status { get; set; }
        public DateTime CreationTime { get; set; }
    }
}