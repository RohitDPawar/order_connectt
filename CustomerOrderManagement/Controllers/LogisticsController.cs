using Microsoft.AspNetCore.Mvc;

namespace AspnetCoreMvcFull.Controllers;

public class LogisticsController : Controller
{
  public IActionResult Dashboard() => View();
  public IActionResult Fleet() => View();
}
