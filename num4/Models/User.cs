using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace num4.Models
{
    public class User : IdentityUser
    {
        public virtual DateTime? LastLoginTime { get; set; }
        public virtual DateTime? RegistrationDate { get; set; }
        public virtual bool Status { get; set; }
    }
}
