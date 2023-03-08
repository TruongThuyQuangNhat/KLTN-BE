using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageUser.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string LastName { get; set; }
        public string Avatar { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; } 
        public bool IsOnline { get; set; }
        public string ConnectionID { get; set; }
        public string RoomID { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
