using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PakProperties.Controllers
{
    public class HostelController : Controller
    {
        [Authorize]
        public IActionResult HostelIndex()
        {
            return View();
        }
    }
}
