using ManageUser.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ManageUser.Model
{
    public class HistoryOfSalary
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public Guid FromSalaryId { get; set; }
        public DateTime Month { get; set; }
    }
}