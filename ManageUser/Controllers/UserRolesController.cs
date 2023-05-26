using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _appDbContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRolesController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext appDbContext,
            RoleManager<IdentityRole> roleManager
        )
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("getlist")]
        public object GetList()
        {
            object ur = _appDbContext.Roles.ToList();
            return ur;
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] updateModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }
            await _userManager.RemoveFromRoleAsync(user, model.OldRoles);
            await _userManager.AddToRoleAsync(user, model.NewRoles);
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật Roles User thành công." });
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> getRolesOfUser(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            var data = await _userManager.GetRolesAsync(user);
            return StatusCode(StatusCodes.Status200OK, data);
        }

        [HttpGet]
        [Route("getusersinrole")]
        public async Task<IActionResult> GetUsersInRole([FromBody] string Roles)
        {
            var data = await _userManager.GetUsersInRoleAsync(Roles);
            return StatusCode(StatusCodes.Status200OK, data);
        }

        [HttpGet]
        [Route("isinrole")]
        public async Task<IActionResult> CheckUserInRoles([FromBody] UserRolesModel2 model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }
            var data = await _userManager.IsInRoleAsync(user, model.RolesName);
            return StatusCode(StatusCodes.Status200OK, data);
        }

        public class updateModel
        {
            [Required]
            public string UserId { set; get; }

            [Required]
            [RegularExpression("^(Admin|HR|Employee)$")]
            public string OldRoles { set; get; }

            [Required]
            [RegularExpression("^(Admin|HR|Employee)$")]
            public string NewRoles { set; get; }
        }

        public class UserRolesModel
        {
            public string UserId { set; get; }
            public string RolesId { set; get; }
        }

        public class UserRolesModel2
        {
            public string UserId { set; get; }
            public string RolesName { set; get; }
        }
    }
}
