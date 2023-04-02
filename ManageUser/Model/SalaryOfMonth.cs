using ManageUser.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class SalaryOfMonth
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public string Money { get; set; }
        public string FuelAllowance { get; set; }
        public string LunchAllowance { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
