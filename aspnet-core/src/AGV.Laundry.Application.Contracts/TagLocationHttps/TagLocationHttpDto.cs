using System;
using AGV.Laundry.Enums;
using Volo.Abp.Application.Dtos;

namespace AGV.Laundry.TagLocationHttps
{
    public class TagLocationHttpDto
    {
        public string cartId { get; set; }
        public string cartNo { get; set; }
        public string lotNo { get; set; }
        public string state { get; set; }
    }
}