using ManageUser.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageUser.Model
{
    public class UserInfo
    {
        [Key]
        public string Id { get; set; }
        public ApplicationUser FromUser { get; set; }
        public string Sex { get; set; }
        public string Address { get; set; }
        public int Age { get; set; }
        public DateTime BirthDay { get; set; }
        public DateTime DateStartWork { get; set; }
        public ApplicationUser Manager { get; set; }
        public string DayOffId { get; set; }
        public string SalaryId { get; set; }
        public string BonusId { get; set; }
        public string AdvanceMoneyId { get; set; }
        public string CCCDNumber { get; set; }
        public DateTime CCCDIssueDate { get; set; }
        public string CCCDAddress { get; set; }
        public string BHXHNumber { get; set; }
        public DateTime BHXHIssueDate { get; set; }
        public DateTime BHXHStartDate { get; set; }
        public string BHYTNumber { get; set; }
        public DateTime BHYTIssueDate { get; set; }
        public string BHYTAddress { get; set; }
        public string BHTNNumber { get; set; }
        public DateTime BHTNIssueDate { get; set; }
        public string SLDNumber { get; set; }
        public string SLDAddress { get; set; }
        public DateTime SLDIssueDate { get; set; }
        public string BankNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountName { get; set; }
        public string HDLDNumber { get; set; }
        public DateTime HDLDStartDate { get; set; }
        public DateTime HDLDEndDate { get; set; }
        public Department Department { get; set; }
        public string PositionId { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}