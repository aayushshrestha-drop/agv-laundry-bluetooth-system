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
        public async Task<ActionResult> AGVLaundry([FromBody]AGVPostDto model)
        {
            return Json(new { success = true });
        }

        public class AGVPostDto
        {
            public string cartNo { get; set; }
            public string lotNo { get; set; }
            public string state { get; set; }
        }
    }
}
