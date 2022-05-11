using AGV.Laundry.BaseStations;
using AGV.Laundry.Permissions;
using AGV.Laundry.Tags;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AGV.Laundry.TagLocationLogs
{
    [Authorize(AGVLaundryPermissions.TagLocationLogs.Default)]
    public class TagLocationLogAppService : ApplicationService,
        ITagLocationLogAppService
    {
        IRepository<Tag, Guid> _tagRepository;
        IRepository<TagLocationLog, Guid> _tagLocationLogRepository;
        IRepository<BaseStation, Guid> _baseStationRepository;
        public TagLocationLogAppService(
            IRepository<Tag, Guid> tagRepository,
            IRepository<TagLocationLog, Guid> tagLocationLogRepository,
            IRepository<BaseStation, Guid> baseStationRepository
            )
        {
            _tagRepository = tagRepository;
            _tagLocationLogRepository = tagLocationLogRepository;
            _baseStationRepository = baseStationRepository;
        }
        public async Task<List<TagDto>> GetTags()
        {
            var tags = await _tagRepository.GetListAsync(w => w.Status);
            var dtos = ObjectMapper.Map<List<Tag>, List<TagDto>>(tags);
            return dtos;
        }
        public async Task<List<TagLocationLogDto>> GetTagLocationLogs(Guid tagId)
        {
            var tagLocationLogs = await _tagLocationLogRepository.GetListAsync();
            var tags = await _tagRepository.GetListAsync();
            var _baseStations = await _baseStationRepository.GetListAsync();
            var records = (from tll in tagLocationLogs
                        join t in tags on tll.TagId equals t.Id
                        join b in _baseStations on tll.BasestationId equals b.Id
                        where t.Id == tagId
                        select new TagLocationLogDto() 
                        {
                            BasestationId = b.Id,
                            BasestationIp = b.BSIP,
                            Hotel = b.Hotel,
                            Lot = b.LotNo,
                            TagId = t.Id,
                            TagMac = t.TagId,
                            Cart = t.CartNo,
                            CreationTime = tll.CreationTime,
                            Status = (Enums.TagLocationLogStatus)tll.Status
                        }).OrderByDescending(o => o.CreationTime).Take(10).ToList();
            return records;
        }
    }
}
