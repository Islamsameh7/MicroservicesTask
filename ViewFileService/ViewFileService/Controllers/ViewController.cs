using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ViewFileService.Data;
using ViewFileService.Models;

namespace ViewFileService.Controllers
{
    [Route("[controller]")]
    public class ViewController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        public ViewController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpPost("download")]
        public IActionResult DownloadFile([FromBody] DbFile fileModel)
        {
            if (fileModel != null)
            {
                return File(fileModel.FileData, fileModel.FileType, fileModel.FileName);
            }

            return NotFound();
        }
    }
}