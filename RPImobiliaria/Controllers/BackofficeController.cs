using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RPImobiliaria.Controllers;

[Authorize(Roles = "Admin")]
public class BackofficeController : Controller
{
    public IActionResult Index() => View();
}
