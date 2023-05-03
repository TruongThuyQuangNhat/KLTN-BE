using ManageUser.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class AdvanceMoney
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public string Approval { get; set; }
        public Guid ApprovelId { get; set; }
        public DateTime AdvanceDate { get; set; }
        public string Note { get; set; }
        public string Money { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
