using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static ManageUser.Controllers.SalaryController;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryOfSalaryController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;

        public HistoryOfSalaryController(
            ApplicationDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateHistorySalary model)
        {
            string Month = model.Date.Month.ToString();
            string Year = model.Date.Year.ToString();
            var user = await _appDbContext.User.FindAsync(model.Id);
            if(user == null)
            {
                return NotFound("User không tồn tại");
            }
            var salary = _appDbContext.SalaryOfMonth.Where(i => i.FromUserId.ToString() == user.Id).FirstOrDefault();
            if (salary == null)
            {
                return NotFound("Salary không tồn tại");
            }
            var dayOff = _appDbContext.DayOff.Where(i => 
                i.FromUserId.ToString().Equals(user.Id) && 
                i.DateOff.Month.ToString().Equals(Month) && 
                i.DateOff.Year.ToString().Equals(Year) &&
                i.Approval.Equals("3")).ToList();
            var bonus = _appDbContext.Bonus.Where(i => 
                i.FromUserId.ToString() == user.Id && 
                i.DateBonus.Month.ToString() == Month && 
                i.DateBonus.Year.ToString() == Year).ToList();
            var advanceMoney  = _appDbContext.AdvanceMoney.Where(i => 
                i.FromUserId.ToString() == user.Id && 
                i.AdvanceDate.Month.ToString() == Month && 
                i.AdvanceDate.Year.ToString() == Year &&
                i.Approval == "3").ToList();

            double money = double.Parse(salary.Money);
            double numberDayOff = 0;
            dayOff.ForEach(i =>
            {
                if (i.HalfDate == "3")
                {
                    numberDayOff++;
                }
                else
                {
                    numberDayOff = numberDayOff + 0.5;
                }
            });
            if(numberDayOff > 0)
            {
                money = money - (money / 24) * numberDayOff;
            }

            bonus.ForEach(i =>
            {
                money += double.Parse(i.Money);
            });

            advanceMoney.ForEach(i =>
            {
                money -= double.Parse(i.Money);
            });

            var lstDayOff = dayOff.Select(i => i.Id).ToList();
            var lstBonus = bonus.Select(i => i.Id).ToList();
            var lstAdvanceMoney = advanceMoney.Select(i => i.Id).ToList();
            HistoryOfSalary data = new HistoryOfSalary();
            data.Id = Guid.NewGuid();
            data.FromUserId = Guid.Parse(model.Id);
            data.FromDayOff = lstDayOff;
            data.FromBonus = lstBonus;
            data.FromAdvance = lstAdvanceMoney;
            data.SalaryDate = DateTime.Now;
            data.Note = "Bảng lương tháng " + Month + " - " + Year + " | " + model.Note;
            data.Money = money.ToString();
            data.FuelAllowance = salary.FuelAllowance;
            data.LunchAllowance = salary.LunchAllowance;
            data.CreateOn = DateTime.Now;
            data.ModifyOn = DateTime.Now;
            await _appDbContext.HistoryOfSalary.AddAsync(data);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, data);
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateHistoryOfSalary model)
        {
            var sa = await _appDbContext.HistoryOfSalary.FindAsync(model.Id);
            if (sa == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin Lịch Sử Lương Tháng không tồn tại!" });
            }
            else
            {
                sa.Note = model.Note;
                sa.ModifyOn = DateTime.Now;
                _appDbContext.Update(sa);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật Ghi chú Lịch Sử Lương Tháng thành công." });
            }
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var sa = await _appDbContext.HistoryOfSalary.FindAsync(Guid.Parse(Id));
            if (sa == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Lịch sử Lương Tháng không tồn tại!" });
            }
            // CHECK thêm mấy bảng bị phụ thuộc với SalaryOfMonth
            _appDbContext.Remove(sa);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa Lịch sử Lương Tháng thành công." });
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var sa = await _appDbContext.HistoryOfSalary.FindAsync(Guid.Parse(Id));
            if (sa == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Lịch sử Lương Tháng không tồn tại!" });
            }
            else
            {
                List<DayOff> dayOff = new List<DayOff>();
                sa.FromDayOff.ForEach(async i =>
                {
                    var d = await _appDbContext.DayOff.FindAsync(i);
                    if(d != null)
                    {
                        dayOff.Add(d);
                    }
                });
                List<Bonus> bonus = new List<Bonus>();
                sa.FromBonus.ForEach(async i =>
                {
                    var d = await _appDbContext.Bonus.FindAsync(i);
                    if (d != null)
                    {
                        bonus.Add(d);
                    }
                });
                List<AdvanceMoney> advanceMoney = new List<AdvanceMoney>();
                sa.FromAdvance.ForEach(async i =>
                {
                    var d = await _appDbContext.AdvanceMoney.FindAsync(i);
                    if (d != null)
                    {
                        advanceMoney.Add(d);
                    }
                });
                return StatusCode(StatusCodes.Status200OK, new {sa, dayOff, bonus, advanceMoney});
            }
        }

        [HttpPost]
        [Route("getlist")]
        public response<ListSalary> GetList([FromBody] GridModel model)
        {
            var sa = _appDbContext.HistoryOfSalary.ToList();
            var users = _appDbContext.User.ToList();
            if (model.listFilter.Count != 0)
            {
                model.listFilter.ForEach(i =>
                {
                    switch (i.filterColumns)
                    {
                        case "FromUserId":
                            sa = _appDbContext.HistoryOfSalary.FromSqlRaw("SELECT * FROM public.\"HistoryOfSalary\" WHERE \"FromUserId\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "FuelAllowance":
                            sa = _appDbContext.HistoryOfSalary.FromSqlRaw("SELECT * FROM public.\"HistoryOfSalary\" WHERE \"FuelAllowance\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "LunchAllowance":
                            sa = _appDbContext.HistoryOfSalary.FromSqlRaw("SELECT * FROM public.\"HistoryOfSalary\" WHERE \"LunchAllowance\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "Money":
                            sa = _appDbContext.HistoryOfSalary.FromSqlRaw("SELECT * FROM public.\"HistoryOfSalary\" WHERE \"Money\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                    }
                });
            }
            if (!String.IsNullOrEmpty(model.searchText))
            {
                sa = (from s in sa
                      join u in users on s.FromUserId.ToString() equals u.Id
                      where u.FirstName.ToLower().Contains(model.searchText.ToLower()) || u.LastName.ToLower().Contains(model.searchText.ToLower())
                      || s.Note.ToLower().Contains(model.searchText.ToLower())
                      select s).ToList();
            }
            if (!String.IsNullOrEmpty(model.srtColumns) && !String.IsNullOrEmpty(model.srtDirections))
            {
                switch (model.srtColumns)
                {
                    case "CreateOn":
                        if (model.srtDirections == "desc")
                        {
                            sa = sa.OrderByDescending(u => u.CreateOn).ToList();
                        }
                        else if (model.srtDirections == "asc")
                        {
                            sa = sa.OrderBy(u => u.CreateOn).ToList();
                        }
                        break;
                }
            }
            var data = (from s in sa
                        join u in users on s.FromUserId.ToString() equals u.Id
                        select new ListSalary()
                        {
                            Id = s.Id,
                            FromUserId = s.FromUserId,
                            Avatar = u.Avatar,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Money = s.Money,
                            FuelAllowance = s.FuelAllowance,
                            LunchAllowance = s.LunchAllowance
                        }).ToList();
            var dataCount = data;
            if (model.pageLoading)
            {
                data = data.Skip(model.pageSize * model.page).Take(model.pageSize).ToList();
            }

            response<ListSalary> result = new response<ListSalary>()
            {
                data = data,
                dataCount = data.Count(),
                page = model.page + 1,
                pageSize = model.pageSize,
                totalPages = Convert.ToInt32(Math.Ceiling(dataCount.Count() / Convert.ToDouble(model.pageSize))),
                totalCount = dataCount.Count()
            };
            return result;
        }

        public class CreateHistorySalary
        {
            public string Id { set; get; }
            public string Note { set; get; }
            public DateTime Date { set; get; }
        }

        public class UpdateHistoryOfSalary
        {
            public Guid Id { set; get; }
            public List<Guid> FromDayOff { set; get; }
            public List<Guid> FromBonus { set; get; }
            public List<Guid> FromAdvance { set; get; }
            public Guid FromUserId { set; get; }
            public DateTime SalaryDate { set; get; }
            public string Note { get; set; }
            public string Money { set; get; }
            public string FuelAllowance { set; get; }
            public string LunchAllowance { get; set; }
        }
    }
}
