using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkUpPro.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }
}
