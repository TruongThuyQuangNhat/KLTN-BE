using ManageUser.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Model
{
    public class Department
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public ApplicationUser ManagerDepartment { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime ModifyOn { get; set; }
    }
}
