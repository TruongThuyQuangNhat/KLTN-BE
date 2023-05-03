using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(
            ApplicationDbContext appDbContext,
            UserManager<ApplicationUser> userManager
        )
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] UserInfo model)
        {
            var user = await _userManager.FindByIdAsync(model.FromUserId.ToString());
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User không tồn tại!" });
            } else
            {
                model.Id = Guid.NewGuid();
                await _appDbContext.UserInfo.AddAsync(model);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Thêm mới User thành công." });

            }
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UserInfo model)
        {
            var userInfo = await _appDbContext.UserInfo.FindAsync(model.Id);
            if(userInfo == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User không tồn tại!" });
            } else
            {
                userInfo.Sex = model.Sex;
                userInfo.Address = model.Address;
                userInfo.Age = model.Age;
                userInfo.BirthDay = model.BirthDay;
                userInfo.DateStartWork = model.DateStartWork;
                userInfo.CCCDNumber = model.CCCDNumber;
                userInfo.CCCDIssueDate = model.CCCDIssueDate;
                userInfo.CCCDAddress = model.CCCDAddress;
                userInfo.BHXHNumber = model.BHXHNumber;
                userInfo.BHXHIssueDate = model.BHXHIssueDate;
                userInfo.BHXHStartDate = model.BHXHStartDate;
                userInfo.BHYTNumber = model.BHYTNumber;
                userInfo.BHYTIssueDate = model.BHYTIssueDate;
                userInfo.BHYTAddress = model.BHYTAddress;
                userInfo.BHTNNumber = model.BHTNNumber;
                userInfo.BHTNIssueDate = model.BHTNIssueDate;
                userInfo.SLDNumber = model.SLDNumber;
                userInfo.SLDAddress = model.SLDAddress;
                userInfo.SLDIssueDate = model.SLDIssueDate;
                userInfo.BankNumber = model.BankNumber;
                userInfo.BankName = model.BankName;
                userInfo.HDLDNumber = model.HDLDNumber;
                userInfo.HDLDStartDate = model.HDLDStartDate;
                userInfo.HDLDEndDate = model.HDLDEndDate;
                userInfo.ModifyOn = DateTime.Now;

                _appDbContext.Update(userInfo);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin User thành công." });
            }
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var userInfo = await _appDbContext.UserInfo.FindAsync(Guid.Parse(Id));
            if (userInfo == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User không tồn tại!" });
            }
            else
            {
                // CHECK thêm mấy bảng bị phụ thuộc User Info
                _appDbContext.Remove(userInfo);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa thông tin User thành công." });
            }
        }

        [HttpGet]
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
        public response<resUser> GetList([FromBody] GridModel model)
        {
            var department = _appDbContext.Department.ToList();
            var position = _appDbContext.Position.ToList();
            var userList = _appDbContext.Users.ToList();
            var userInfo = _appDbContext.UserInfo.ToList();
            var userRoles = _appDbContext.UserRoles.ToList();
            var roles = _appDbContext.Roles.ToList();
            if (model.listFilter.Count != 0)
            {
                model.listFilter.ForEach(i =>
                {
                    switch (i.filterColumns)
                    {
                        case "Position":
                            position = _appDbContext.Position.FromSqlRaw("SELECT * FROM public.\"Position\" WHERE \"Id\""+i.filterDirections+"'"+ i.filterData + "'").ToList();
                            break;
                        case "Department":
                            department = _appDbContext.Department.FromSqlRaw("SELECT * FROM public.\"Department\" WHERE \"Id\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                    }
                });
            }
            if (!String.IsNullOrEmpty(model.searchText))
            {
                userList = userList.Where(u => u.FirstName.Contains(model.searchText) || u.LastName.Contains(model.searchText)).ToList();
            }
            if(!String.IsNullOrEmpty(model.srtColumns) && !String.IsNullOrEmpty(model.srtDirections))
            {
                switch (model.srtColumns)
                {
                    case "FirstName":
                        if(model.srtDirections == "desc")
                        {
                            userList = userList.OrderByDescending(u => u.FirstName).ToList();
                        } else if(model.srtDirections == "asc")
                        {
                            userList = userList.OrderBy(u => u.FirstName).ToList();
                        }
                        break;
                    case "LastName":
                        if (model.srtDirections == "desc")
                        {
                            userList = userList.OrderByDescending(u => u.LastName).ToList();
                        }
                        else if (model.srtDirections == "asc")
                        {
                            userList = userList.OrderBy(u => u.LastName).ToList();
                        }
                        break;
                }
            }
            var list = from u in userList
                       join ui in userInfo on u.Id equals ui.FromUserId.ToString()
                       join de in department on u.DepartmentId equals de.Id
                       join po in position on u.PositionId equals po.Id
                       join ur in userRoles on u.Id equals ur.UserId
                       join r in roles on ur.RoleId equals r.Id
                       select new resUser
                       {
                           Id = u.Id,
                           LastName = u.LastName,
                           FirstName = u.FirstName,
                           Email = u.Email,
                           Avatar = u.Avatar,
                           DepartmentName = de.Name,
                           PositionName = po.Name,
                           Roles = r.Name
                       };
            var data = list;
            if (model.pageLoading)
            {
                list = list.Skip(model.pageSize * model.page).Take(model.pageSize).ToList();
            }

            response<resUser> result = new response<resUser>()
            {
                data = list,
                dataCount = list.Count(),
                page = model.page + 1,
                pageSize = model.pageSize,
                totalPages = Convert.ToInt32(Math.Ceiling(data.Count() / Convert.ToDouble(model.pageSize)))
            };
            return result;
        }
    }

    public class response<T>
    {
        public IEnumerable<T> data { set; get; }
        public int page { set; get; }
        public int pageSize { set; get; }
        public int totalPages { set; get; }
        public int dataCount { set; get; }
    }

    public class resUser
    {
        public string Id { set; get; }
        public string LastName { set; get; }
        public string FirstName { set; get; }
        public string Email { set; get; }
        public string Avatar { set; get; }
        public string DepartmentName { set; get; }
        public string PositionName { set; get; }
        public string Roles { set; get; }
    }
}
