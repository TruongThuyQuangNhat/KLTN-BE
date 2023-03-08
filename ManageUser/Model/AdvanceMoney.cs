using ManageUser.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class AdvanceMoney
    {
        [Key]
        public string Id { get; set; }
        public ApplicationUser FromUser { get; set; }
        public string Approval { get; set; }
        public ApplicationUser Approvel { get; set; }
        public string Money { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
