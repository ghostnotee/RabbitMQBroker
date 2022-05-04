using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RabbitMQWeb.WordToPdf.Models;
using RabbitMQWeb.WordToPdf.Services;

namespace RabbitMQWeb.WordToPdf.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly RabbitMqPublisher _rabbitMqPublisher;

    public HomeController(ILogger<HomeController> logger, RabbitMqPublisher rabbitMqPublisher)
    {
        _logger = logger;
        _rabbitMqPublisher = rabbitMqPublisher;
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
        _rabbitMqPublisher.Publish(wordToPdf);
        ViewBag.result = "Pdf dosyanız mail yoluyla iletilecektir.";
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