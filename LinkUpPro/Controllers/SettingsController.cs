using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

