using AGV.Laundry.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AGV.Laundry.Tags
{
    [Authorize(AGVLaundryPermissions.Tags.Default)]
    public class TagAppService :
        CrudAppService<
            Tag,
            TagDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateTagDto>,
        ITagAppService
    {
        public TagAppService(
            IRepository<Tag, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AGVLaundryPermissions.Tags.Default;
            GetListPolicyName = AGVLaundryPermissions.Tags.Default;
            CreatePolicyName = AGVLaundryPermissions.Tags.Create;
            UpdatePolicyName = AGVLaundryPermissions.Tags.Edit;
            DeletePolicyName = AGVLaundryPermissions.Tags.Create;
        }
    }
}
