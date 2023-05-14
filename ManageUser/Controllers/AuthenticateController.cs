using ManageUser.Authentication;
using ManageUser.Mail;
using ManageUser.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ISendMailService _sendMailService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _appDbContext;

        public AuthenticateController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ISendMailService sendMailService,
            IWebHostEnvironment hostingEnvironment,
            ApplicationDbContext appDbContext
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _sendMailService = sendMailService;
            _hostingEnvironment = hostingEnvironment;
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = CreateToken(authClaims);
                var refreshToken = GenerateRefreshToken();

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if(model.Password != model.RepeatPassword)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Mật khẩu lặp lại không giống nhau!" });
            }

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Name đã tồn tại!" });
            }

            var userExistsEmail = await _userManager.FindByEmailAsync(model.Email);
            if(userExistsEmail != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Email đã tồn tại!" });
            }

            ApplicationUser user = new()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                PositionId = Guid.Parse(model.PositionId),
                DepartmentId = Guid.Parse(model.DepartmentId),
                Avatar = model.Avatar
            };
            UserInfo ui = new UserInfo();
            ui.Id = Guid.NewGuid();
            ui.FromUserId = Guid.Parse(user.Id);
            await _appDbContext.UserInfo.AddAsync(ui);
            await _appDbContext.SaveChangesAsync();
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }
            var token = HttpUtility.UrlEncode(await _userManager.GenerateEmailConfirmationTokenAsync(user));
            var confirmationlink = "https://localhost:5001/api/authenticate/ConfirmEmailLink?token=" + token + "&email=" + user.Email;
            MailContent content = new MailContent
            {
                To = model.Email,
                Subject = "[Jora] Xác Nhận Email Đăng Ký Tài Khoản",
                Body = "<p><strong>Xin chào " + model.Username + "</strong>, hãy nhấn vào <a href=" + confirmationlink + ">link này</a> để xác nhận địa chỉ email của bạn.</p>"
            };

            await _sendMailService.SendMail(content);

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.Employee))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Employee));
            if (!await _roleManager.RoleExistsAsync(UserRoles.HR))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.HR));

            if (await _roleManager.RoleExistsAsync(UserRoles.Employee))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Employee);
            }

            return Ok(new { Status = "Success", Message = "User created successfully!", Data = user });
        }
        [HttpGet]
        [Route("ConfirmEmailLink")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return Ok(new Response { Status = "Success", Message = "User comfirm email successfully!" });
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpPost]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.email);
            if (userExists != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(userExists);
                var link = "https://localhost:44350/api/authenticate/confirmresetpassword?";
                var buillink = link + "&Id=" + userExists.Id + "&token=" + token;
                MailContent content = new MailContent
                {
                    To = userExists.Email,
                    Subject = "[Jora] Xác Nhận Email Reset Password",
                    Body = "<p><strong>Xin chào " + userExists.UserName + "</strong>, hãy nhấn vào <a href=" + buillink + ">link này</a> để tạo Password mới cho tài khoản của bạn.</p>"
                };
                await _sendMailService.SendMail(content);
                return Ok(new Response { Status = "Success", Message = "Send Email Reset Password successfully!" });
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("confirmresetpassword")]
        public async Task<IActionResult> ConfirmResetPassword([FromBody] ConfirmResetPassword model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if(user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.token, model.newpassword);
                if (result.Succeeded)
                {
                    return Ok(new Response { Status = "Success", Message = "Reset Password successfully!" });
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return NotFound();
            }
            
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            if (model.Password != model.RepeatPassword)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Mật khẩu lặp lại không giống nhau!" });
            }

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User Name đã tồn tại!" });
            }

            var userExistsEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userExistsEmail != null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Email đã tồn tại!" });
            }

            ApplicationUser user = new()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username,
                PositionId = Guid.Parse(model.PositionId),
                DepartmentId = Guid.Parse(model.DepartmentId),
                Avatar = model.Avatar
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.Employee))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Employee));
            if (!await _roleManager.RoleExistsAsync(UserRoles.HR))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.HR));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            /*if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }*/
            /*var token = HttpUtility.UrlEncode(await _userManager.GenerateEmailConfirmationTokenAsync(user));
            var confirmationlink = "https://localhost:5001/api/authenticate/ConfirmEmailLink?token=" + token + "&email=" + user.Email;
            MailContent content = new MailContent
            {
                To = model.Email,
                Subject = "[Jora] Xác Nhận Email Đăng Ký Tài Khoản",
                Body = "<p><strong>Xin chào " + model.Username + "</strong>, hãy nhấn vào <a href=" + confirmationlink + ">link này</a> để xác nhận địa chỉ email của bạn.</p>"
            };

            await _sendMailService.SendMail(content);*/

            return Ok(new { Status = "Success", Message = "User created successfully!", Data = user });
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return BadRequest("Invalid access token or refresh token");
            }
            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token or refresh token");
            }

            var newAccessToken = CreateToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [Authorize]
        [HttpPost]
        [Route("revoke/{username}")]
        public async Task<IActionResult> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Invalid user name");

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [Authorize]
        [HttpPost]
        [Route("revoke-all")]
        public async Task<IActionResult> RevokeAll()
        {
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                user.RefreshToken = null;
                await _userManager.UpdateAsync(user);
            }

            return NoContent();
        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;

        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> UpdateUser([FromBody] updateUser model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if(user == null)
            {
                return NotFound();
            } else
            {
                user.Avatar = model.Avatar;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                await _userManager.UpdateAsync(user);
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật User thành công." });
            }
        }

        [HttpPost]
        [Route("delete/{Id}")]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                await _userManager.DeleteAsync(user);
                // check thêm mấy bảng phụ thuộc
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa User thành công." });
            }
        }

        [HttpPost]
        [Route("upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFile()
        {
            if (!Request.Form.Files.Any())
                return BadRequest("No files found in the request");

            if (Request.Form.Files.Count > 1)
                return BadRequest("Cannot upload more than one file at a time");

            if (Request.Form.Files[0].Length <= 0)
                return BadRequest("Invalid file length, seems to be empty");

            try
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                string uploadsDir = Path.Combine(webRootPath, "images");

                // wwwroot/uploads/
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                IFormFile file = Request.Form.Files[0];
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                Random rnd = new Random();
                string random = rnd.Next(11111111, 99999999).ToString();
                string fullPath = Path.Combine(uploadsDir, random + fileName);

                var buffer = 1024 * 1024;
                using var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, buffer, useAsync: false);
                await file.CopyToAsync(stream);
                await stream.FlushAsync();

                string location = $"images/{random + fileName}";

                var result = new
                {
                    message = "Upload successful",
                    url = location
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Upload failed: " + ex.Message);
            }
        }

        public class updateUser
        {
            public string Id { set; get; }
            public string FirstName { set; get; }
            public string LastName { set; get; }
            public string Avatar { set; get; }
            public string Email { set; get; }
            public string PhoneNumber { set; get; }
        }
    }
}
