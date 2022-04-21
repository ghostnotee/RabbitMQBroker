using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreating.Models;
using RabbitMQWeb.ExcelCreating.Services;

namespace RabbitMQWeb.ExcelCreating.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly RabbitMqPublisher _rabbitMqPublisher;

        public ProductController(UserManager<IdentityUser> userManager, AppDbContext context, RabbitMqPublisher rabbitMqPublisher)
        {
            _userManager = userManager;
            _context = context;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";
            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };
            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();
            _rabbitMqPublisher.Publish(new Shared.CreateExcelMessage()
            {
                FileId = userFile.Id,
                UserId = user.Id
            });
            TempData["StartCreatingExcel"] = true;

            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return View(await _context.UserFiles.Where(uF => uF.UserId == user.Id).ToListAsync());
        }
    }
}