using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageUser.Authentication
{
    public class ResetPasswordModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string email { get; set; }
    }

    public class ConfirmResetPassword
    {
        [Required(ErrorMessage = "id is required")]
        public string Id { get; set; }

        [Required(ErrorMessage = "token is required")]
        public string token { get; set; }

        [Required(ErrorMessage = "newpassword is required")]
        public string newpassword { get; set; }
    }
}
