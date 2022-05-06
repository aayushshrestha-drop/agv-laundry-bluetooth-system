using AGV.Laundry.Tags;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace AGV.Laundry.Controllers
{
    public class HomeController : AbpController
    {
        private ITagAppService _tagAppService { get; set; }
        public HomeController(ITagAppService tagAppService)
        {
            _tagAppService = tagAppService;
        }
        public ActionResult Index()
        {
            return Redirect("~/swagger");
        }
    }
}
