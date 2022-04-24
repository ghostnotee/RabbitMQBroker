using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQWeb.ExcelCreating.Models;

namespace RabbitMQWeb.ExcelCreating.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FilesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)
        {
            if (file is not {Length: > 0}) return BadRequest();

            var userFile = await _context.UserFiles.FirstAsync(f => f.Id == fileId);
            var fileName = userFile.FileName + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);
            await using FileStream stream = new(path, FileMode.Create);
            await file.CopyToAsync(stream);

            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = path;
            userFile.FileStatus = FileStatus.Completed;

            await _context.SaveChangesAsync();

            // TODO SignalR notification atılacak.

            return Ok();
        }
    }
}