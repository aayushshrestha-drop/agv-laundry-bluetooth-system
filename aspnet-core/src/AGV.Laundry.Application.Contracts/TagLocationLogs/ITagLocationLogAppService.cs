using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AGV.Laundry.Tags;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AGV.Laundry.TagLocationLogs
{
    public interface ITagLocationLogAppService
    {
        Task<List<TagDto>> GetTags();
        Task<List<TagLocationLogDto>> GetTagLocationLogs(Guid tagId);
    }
}
