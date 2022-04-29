using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RabbitMQWeb.WordToPdf.Models;

namespace RabbitMQWeb.WordToPdf.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult WordToPdfPage()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult WordToPdfPage(WordToPdfModel wordToPdf)
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}