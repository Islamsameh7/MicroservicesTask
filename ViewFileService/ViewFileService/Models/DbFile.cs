using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViewFileService.Models
{
    public class DbFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] FileData { get; set; }

    }
}