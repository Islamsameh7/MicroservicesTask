using Microsoft.AspNetCore.Mvc;
using UploadFileService.Data;
using UploadFileService.Models;

namespace UploadService.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext db;
    public HomeController(ApplicationDbContext db)
    {
        this.db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("uploadfile")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                var fileData = stream.ToArray();

                var fileModel = new DbFile
                {
                    FileName = file.FileName,
                    FileType = file.ContentType,
                    FileData = fileData
                };

                await db.DbFiles.AddAsync(fileModel);
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
        }
        return View();
    }

    [HttpGet("GetFile/{id}")]
    public async Task<ActionResult> GetFile(int id)
    {
        var file = await db.DbFiles.FindAsync(id);

        if (file is null) return NotFound("File Not Found");

        return Ok(file);

    }

}
