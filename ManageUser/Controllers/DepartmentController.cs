using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;

        public DepartmentController(
            ApplicationDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateDepartment model)
        {
            Department department = new Department();
            department.Name = model.Name;
            department.ManagerDepartmentId = Guid.Parse(model.ManagerDepartmentId);
            department.Id = Guid.NewGuid();
            department.CreateOn = DateTime.Now;
            department.ModifyOn = DateTime.Now;
            await _appDbContext.Department.AddAsync(department);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Thêm mới Department thành công." });
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateDepartment model)
        {
            var department = await _appDbContext.Department.FindAsync(Guid.Parse(model.Id));
            if (department == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Phòng Ban không tồn tại!" });
            }
            else
            {
                department.Name = model.Name;
                department.ManagerDepartmentId = Guid.Parse(model.ManagerDepartmentId);
                department.ModifyOn = DateTime.Now;
                _appDbContext.Update(department);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin Phòng Ban thành công." });
            }
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var department = await _appDbContext.Department.FindAsync(Guid.Parse(Id));
            if (department == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Phòng Ban không tồn tại!" });
            }
            else
            {
                // CHECK thêm mấy bảng bị phụ thuộc với department
                _appDbContext.Remove(department);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa thông tin Phòng Ban thành công." });
            }
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var department = await _appDbContext.Department.FindAsync(Guid.Parse(Id));
            if (department == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Phòng Ban không tồn tại!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, department);
            }
        }

        [HttpPost]
        [Route("getlist")]
        public response<resDepartment> GetList([FromBody] GridModel model)
        {
            var department = _appDbContext.Department.ToList();
            var users = _appDbContext.User.ToList();
            if (!String.IsNullOrEmpty(model.searchText))
            {
                department = department.Where(u => u.Name.Contains(model.searchText)).ToList();
            }
            var list = from de in department
                       /*join u in users on de.ManagerDepartmentId equals Guid.Parse(u.Id)*/
                       select new resDepartment()
                       {
                           Id = de.Id,
                           Name = de.Name,
                           Avatar = "u.Avatar",
                           Manager = "u.LastName  u.FirstName"
                       };

            var data = list;
            if (model.pageLoading)
            {
                list = list.Skip(model.pageSize * model.page).Take(model.pageSize).ToList();
            }

            response<resDepartment> result = new response<resDepartment>()
            {
                data = list,
                dataCount = list.Count(),
                page = model.page + 1,
                pageSize = model.pageSize,
                totalPages = Convert.ToInt32(Math.Ceiling(data.Count() / Convert.ToDouble(model.pageSize))),
                totalCount = data.Count()
            };
            return result;
        }
    }

    public class CreateDepartment
    {
        public string Name { get; set; }
        public string ManagerDepartmentId { get; set; }
    }

    public class UpdateDepartment
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ManagerDepartmentId { get; set; }
    }

    public class resDepartment
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string Manager { get; set; }
    }
}
