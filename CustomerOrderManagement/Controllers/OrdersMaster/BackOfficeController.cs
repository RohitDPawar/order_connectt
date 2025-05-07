using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderManagement.Controllers.OrdersMaster
{
  public class BackOfficeController : Controller
  {

    public IActionResult Index()
    {
      return View();
    }

    public IActionResult MainDashboard()
    {
      return View();
    }
  }
}
