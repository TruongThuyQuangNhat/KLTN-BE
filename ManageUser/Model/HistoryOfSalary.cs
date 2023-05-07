using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class HistoryOfSalary
    {
        [Key]
        public Guid Id { set; get; }
        public List<Guid> FromDayOff { set; get; }
        public List<Guid> FromBonus { set; get; }
        public List<Guid> FromAdvance { set; get; }
        public Guid FromUserId { set; get; }
        public DateTime SalaryDate { set; get; }
        public string Note { get; set; }
        public string Money { set; get; }
        public string FuelAllowance { set; get; }
        public string LunchAllowance { get; set; }
        public DateTime CreateOn { set; get; }
        public DateTime ModifyOn { set; get; }
    }
}
