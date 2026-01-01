using Microsoft.AspNetCore.Mvc;

namespace PreuveTierce.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
