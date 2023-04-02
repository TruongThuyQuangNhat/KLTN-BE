﻿using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

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
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "User không tồn tại!" });
            } else
            {
                model.Id = Guid.NewGuid();
                model.FromUserId = Guid.Parse(user.Id);
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
                userInfo.ManagerId = model.ManagerId;
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
    }
}
