using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DayOffController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _appDbContext;

        public DayOffController(
            ApplicationDbContext appDbContext,
            UserManager<ApplicationUser> userManager
        )
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if(user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Không tìm thấy user." });
            }
            DayOff dayOff = new DayOff();
            dayOff.Id = Guid.NewGuid();
            dayOff.FromUserId = Guid.Parse(user.Id);
            dayOff.DateOff = model.DateOff;
            dayOff.HalfDate = model.HalfDate;
            dayOff.Approval = "1";
            dayOff.Note = model.Note;
            dayOff.SabbaticalDayOff = model.SabbaticalDayOff;
            dayOff.CreateOn = DateTime.Now;
            dayOff.ModifyOn = DateTime.Now;
            await _appDbContext.DayOff.AddAsync(dayOff);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Thêm mới Ngày Nghỉ thành công." });
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateModel model)
        {
            var dayOff = await _appDbContext.DayOff.FindAsync(model.Id);
            if (dayOff == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Ngày Nghỉ không tồn tại!" });
            }
            if (dayOff.Approval != "1")
            {
                return StatusCode(StatusCodes.Status409Conflict, new Response { Status = "Error", Message = "Ngày Nghỉ ở trạng thái không được chỉnh sửa!" });
            }

            dayOff.DateOff = model.DateOff;
            dayOff.HalfDate = model.HalfDate;
            dayOff.Note = model.Note;
            dayOff.SabbaticalDayOff = model.SabbaticalDayOff;
            dayOff.ModifyOn = DateTime.Now;
            _appDbContext.Update(dayOff);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin Ngày Nghỉ thành công." });
        }

        [HttpPut]
        [Route("approval")]
        public async Task<IActionResult> Approval([FromBody] ApprovalModel model)
        {
            var dayOff = await _appDbContext.DayOff.FindAsync(model.Id);
            if (dayOff == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Ngày Nghỉ không tồn tại!" });
            }
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Không tìm thấy user." });
            }
            dayOff.Approval = model.status;
            dayOff.ModifyOn = DateTime.Now;
            dayOff.ApprovelId = Guid.Parse(user.Id);
            _appDbContext.Update(dayOff);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật trạng thái Ngày Nghỉ thành công." });
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var dayOff = await _appDbContext.DayOff.FindAsync(Guid.Parse(Id));
            if (dayOff == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Ngày Nghỉ không tồn tại!" });
            }
            if (dayOff.Approval != "1")
            {
                return StatusCode(StatusCodes.Status409Conflict, new Response { Status = "Error", Message = "Ngày Nghỉ ở trạng thái không được xóa!" });
            }

            // CHECK thêm mấy bảng bị phụ thuộc với dayOff
            _appDbContext.Remove(dayOff);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa Ngày Nghỉ thành công." });
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var Valued = await _appDbContext.DayOff.FindAsync(Guid.Parse(Id));
            var user = _appDbContext.User;
            if (Valued == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Ngày Nghỉ không tồn tại!" });
            }
            else
            {
                var u = user.Where(i => i.Id == Valued.FromUserId.ToString()).FirstOrDefault();
                var u2 = user.Where(i => i.Id == Valued.ApprovelId.ToString()).FirstOrDefault();
                return StatusCode(StatusCodes.Status200OK, new
                {
                    Id= Valued.Id,
                    Name= u.LastName + " " + u.FirstName,
                    Avatar = u.Avatar,
                    DateOff = Valued.DateOff,
                    HalfDate = Valued.HalfDate,
                    Approval = Valued.Approval,
                    Note = Valued.Note,
                    Name2 = u2!=null?(u2.LastName + " " + u2.FirstName):null,
                    Avatar2 = u2!=null?u2.Avatar:null,
                    SabbaticalDayOff = Valued.SabbaticalDayOff,
                });
            }
        }

        [HttpGet]
        [Route("getSabbatical")]
        public async Task<IActionResult> GetNumberOfSabbatical()
        {
            Double number = 0;
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Không tìm thấy user." });
            }
            var dayOff = _appDbContext.DayOff.ToList();
            var year = DateTime.Now.Year;
            dayOff = dayOff.Where(i => i.FromUserId.ToString() == user.Id).Where(i => i.DateOff.Year == year).ToList();
            foreach(var i in dayOff)
            {
                if(i.HalfDate == "1" || i.HalfDate == "2")
                {
                    number = number + 0.5;
                } else if(i.HalfDate == "3")
                {
                    number = number + 1;
                }
            }
            return StatusCode(StatusCodes.Status200OK, number);
        }

        [HttpPost]
        [Route("getlist")]
        public response<ResDayOff> GetList([FromBody] GridModel model)
        {
            var dayOff = _appDbContext.DayOff.ToList();
            var users = _appDbContext.User.ToList();
            if (model.listFilter.Count != 0)
            {
                model.listFilter.ForEach(i =>
                {
                    if (!String.IsNullOrEmpty(i.filterDirections))
                    {
                        switch (i.filterColumns)
                        {
                            case "FromUserId":
                                dayOff = _appDbContext.DayOff.FromSqlRaw("SELECT * FROM public.\"DayOff\" WHERE \"FromUserId\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "ApprovelId":
                                dayOff = _appDbContext.DayOff.FromSqlRaw("SELECT * FROM public.\"DayOff\" WHERE \"ApprovelId\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "DateOff":
                                dayOff = _appDbContext.DayOff.FromSqlRaw("SELECT * FROM public.\"DayOff\" WHERE \"DateOff\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "HalfDate":
                                dayOff = _appDbContext.DayOff.FromSqlRaw("SELECT * FROM public.\"DayOff\" WHERE \"HalfDate\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "Approval":
                                dayOff = _appDbContext.DayOff.FromSqlRaw("SELECT * FROM public.\"DayOff\" WHERE \"Approval\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                        }
                    } else if (!String.IsNullOrEmpty(i.filterData))
                    {
                        switch (i.filterColumns)
                        {
                            case "FromDate":
                                var date = Convert.ToDateTime(i.filterData);
                                dayOff = dayOff.Where(d => d.DateOff >= date).ToList();
                                break;
                            case "ToDate":
                                var date2 = Convert.ToDateTime(i.filterData);
                                dayOff = dayOff.Where(d => d.DateOff <= date2).ToList();
                                break;
                        }
                    }
                });
            }
            if (!String.IsNullOrEmpty(model.searchText))
            {
                dayOff = dayOff.Where(u => u.Note.ToLower().Contains(model.searchText.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(model.srtColumns) && !String.IsNullOrEmpty(model.srtDirections))
            {
                switch (model.srtColumns)
                {
                    case "DateOff":
                        if (model.srtDirections == "desc")
                        {
                            dayOff = dayOff.OrderByDescending(u => u.DateOff).ToList();
                        }
                        else if (model.srtDirections == "asc")
                        {
                            dayOff = dayOff.OrderBy(u => u.DateOff).ToList();
                        }
                        break;
                }
            }
            dayOff = dayOff.OrderByDescending(u => u.CreateOn).ToList();
            var data = dayOff;
            if (model.pageLoading)
            {
                dayOff = dayOff.Skip(model.pageSize * model.page).Take(model.pageSize).ToList();
            }

            var resDayOff = from i in dayOff
                            join u in users on i.FromUserId.ToString() equals u.Id
                            select new ResDayOff
                            {
                                Id = i.Id,
                                Name = u.LastName + " " + u.FirstName,
                                DateOff = i.DateOff,
                                HalfDate = i.HalfDate,
                                Approval = i.Approval,
                                Note = i.Note,
                                ApprovelId = i.ApprovelId.ToString(),
                                SabbaticalDayOff = i.SabbaticalDayOff,
                                CreateOn = i.CreateOn,
                                ModifyOn = i.ModifyOn

                            };

            response <ResDayOff> result = new response<ResDayOff>()
            {
                data = resDayOff,
                dataCount = dayOff.Count(),
                page = model.page + 1,
                pageSize = model.pageSize,
                totalPages = Convert.ToInt32(Math.Ceiling(data.Count() / Convert.ToDouble(model.pageSize))),
                totalCount = data.Count()
            };
            return result;
        }

        public class CreateModel
        {
            public Guid FromUserId { get; set; }
            public DateTime DateOff { get; set; }
            public string HalfDate { get; set; }
            public string Note { get; set; }
            public Guid ApprovelId { get; set; }
            public bool SabbaticalDayOff { get; set; }
        }

        public class UpdateModel
        {
            public Guid Id { get; set; }
            public DateTime DateOff { get; set; }
            public string HalfDate { get; set; }
            public string Note { get; set; }
            public bool SabbaticalDayOff { get; set; }
        }

        public class ApprovalModel
        {
            public Guid Id { get; set; }
            [RegularExpression("^(1|2|3)$")]
            public string status { get; set; }
        }

        public class ResDayOff
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime DateOff { get; set; }
            public string HalfDate { get; set; }
            public string Approval { get; set; }
            public string Note { get; set; }
            public string ApprovelId { get; set; }
            public bool SabbaticalDayOff { get; set; }
            public DateTime CreateOn { get; set; }
            public DateTime ModifyOn { get; set; }
        }
    }
}
