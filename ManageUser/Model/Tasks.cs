using System.ComponentModel.DataAnnotations;
using System;

namespace ManageUser.Model
{
    public class Tasks
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromUserId { get; set; }
        public string TaskName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid Assigner { get; set; }
        public string Status { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
