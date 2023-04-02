using ManageUser.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class DayOff
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public DateTime DateOff { get; set; }
        public string HalfDate { get; set; }
        public string Approval { get; set; }
        public Guid ApprovelId { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
