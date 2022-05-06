using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace AGV.Laundry
{
    [Authorize]
    public class ChatHub : AbpHub
    {
        private readonly IRepository<Volo.Abp.Identity.IdentityUser> _identityUserRepository;
        //private readonly IIdentityUserRepository _identityUserRepository;
        private readonly ILookupNormalizer _lookupNormalizer;

        public ChatHub(
            //IIdentityUserRepository identityUserRepository, 
            ILookupNormalizer lookupNormalizer,
            IRepository<Volo.Abp.Identity.IdentityUser> identityUserRepository)
        {
            _identityUserRepository = identityUserRepository;
            _lookupNormalizer = lookupNormalizer;
        }

        public async Task SendMessage(string message)
        {
            var targetUsers = await _identityUserRepository.ToListAsync();

            await Clients
                .Users(targetUsers.Select(s => s.Id.ToString()).ToList())
                .SendAsync("ReceiveMessage", message);
        }
    }
}
