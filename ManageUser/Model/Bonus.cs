using ManageUser.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class Bonus
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public string Description { get; set; }
        public DateTime DateBonus { get; set; }
        public string Money { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
