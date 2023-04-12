using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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

        /*[HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var userInfo = await _appDbContext.UserInfo.FindAsync(Guid.Parse(Id));
            if (userInfo == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User không tồn tại!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, userInfo);
            }
        }

        [HttpGet]
        [Route("getlist")]
        public IEnumerable<response> GetList([FromBody] GridModel model)
        {
            var department = _appDbContext.Department.ToList();
            var position = _appDbContext.Position.ToList();
            var userList = _appDbContext.Users.ToList();
            var userInfo = _appDbContext.UserInfo.ToList();
            var list = from u in userList
                       join ui in userInfo on u.Id equals ui.FromUserId.ToString()
                       join de in department on u.DepartmentId equals de.Id
                       join po in position on u.PositionId equals po.Id
                       select new response
                       {
                           LastName = u.LastName,
                           FirstName = u.FirstName,
                           Email = u.Email,
                           Avatar = u.Avatar,
                           DepartmentName = de.Name,
                           PositionName = po.Name
                       };

            return list;
        }*/
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
}
