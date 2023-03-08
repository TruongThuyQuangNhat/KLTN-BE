using ManageUser.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class DayOff
    {
        [Key]
        public string Id { get; set; }
        public ApplicationUser FromUser { get; set; }
        public DateTime DateOff { get; set; }
        public string HalfDate { get; set; }
        public string Approval { get; set; }
        public ApplicationUser Approvel { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
