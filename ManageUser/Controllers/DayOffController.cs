using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DayOffController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;

        public DayOffController(
            ApplicationDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateModel model)
        {
            DayOff dayOff = new DayOff();
            dayOff.Id = Guid.NewGuid();
            dayOff.FromUserId = model.FromUserId;
            dayOff.DateOff = model.DateOff;
            dayOff.HalfDate = model.HalfDate;
            dayOff.Approval = "1";
            dayOff.ApprovelId = model.ApprovelId;
            dayOff.Note = model.Note;
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
            dayOff.ModifyOn = DateTime.Now;
            _appDbContext.Update(dayOff);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin Ngày Nghỉ thành công." });
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
            var dayOff = await _appDbContext.DayOff.FindAsync(Guid.Parse(Id));
            if (dayOff == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Ngày Nghỉ không tồn tại!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, dayOff);
            }
        }

        [HttpGet]
        [Route("getlist")]
        public response<DayOff> GetList([FromBody] GridModel model)
        {
            var dayOff = _appDbContext.DayOff.ToList();
            if (model.listFilter.Count != 0)
            {
                model.listFilter.ForEach(i =>
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
                        case "FromDate":
                            dayOff = _appDbContext.DayOff.FromSqlRaw("SELECT * FROM public.\"DayOff\" WHERE \"DateOff\" >= '" + i.filterData + "'").ToList();
                            break;
                        case "ToDate":
                            dayOff = _appDbContext.DayOff.FromSqlRaw("SELECT * FROM public.\"DayOff\" WHERE \"DateOff\" <= '" + i.filterData + "'").ToList();
                            break;
                    }
                });
            }
            if (!String.IsNullOrEmpty(model.searchText))
            {
                dayOff = dayOff.Where(u => u.Note.Contains(model.searchText)).ToList();
            }
            if (!String.IsNullOrEmpty(model.srtColumns) && !String.IsNullOrEmpty(model.srtDirections))
            {
                switch (model.srtColumns)
                {
                    case "CreateOn":
                        if (model.srtDirections == "desc")
                        {
                            dayOff = dayOff.OrderByDescending(u => u.CreateOn).ToList();
                        }
                        else if (model.srtDirections == "asc")
                        {
                            dayOff = dayOff.OrderBy(u => u.CreateOn).ToList();
                        }
                        break;
                }
            }
            var data = dayOff;
            if (model.pageLoading)
            {
                dayOff = dayOff.Skip(model.pageSize * model.page).Take(model.pageSize).ToList();
            }

            response<DayOff> result = new response<DayOff>()
            {
                data = dayOff,
                dataCount = dayOff.Count(),
                page = model.page + 1,
                pageSize = model.pageSize,
                totalPages = Convert.ToInt32(Math.Ceiling(data.Count() / Convert.ToDouble(model.pageSize)))
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
        }

        public class UpdateModel
        {
            public Guid Id { get; set; }
            public Guid FromUserId { get; set; }
            public DateTime DateOff { get; set; }
            public string HalfDate { get; set; }
            public string Note { get; set; }
            public Guid ApprovelId { get; set; }
        }
    }
}
