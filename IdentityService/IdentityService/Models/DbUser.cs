using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Models
{
    public class DbUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}