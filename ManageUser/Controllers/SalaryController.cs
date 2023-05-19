using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;

        public SalaryController(
            ApplicationDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateSalary model)
        {
            SalaryOfMonth sa = new SalaryOfMonth();
            sa.Id = Guid.NewGuid();
            sa.FromUserId = model.FromUserId;
            sa.Money = model.Money;
            sa.FuelAllowance = model.FuelAllowance;
            sa.LunchAllowance = model.LunchAllowance;
            sa.CreateOn = DateTime.Now;
            sa.ModifyOn = DateTime.Now;
            await _appDbContext.SalaryOfMonth.AddAsync(sa);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Thêm thông tin Lương Tháng thành công." });
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateSalary model)
        {
            var sa = await _appDbContext.SalaryOfMonth.FindAsync(model.Id);
            if (sa == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin Lương Tháng không tồn tại!" });
            }
            else
            {
                sa.Money = model.Money;
                sa.FuelAllowance = model.FuelAllowance;
                sa.LunchAllowance = model.LunchAllowance;
                sa.ModifyOn = DateTime.Now;
                _appDbContext.Update(sa);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin Lương Tháng thành công." });
            }
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var sa = await _appDbContext.SalaryOfMonth.FindAsync(Guid.Parse(Id));
            if (sa == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin Lương Tháng không tồn tại!" });
            }
            // CHECK thêm mấy bảng bị phụ thuộc với SalaryOfMonth
            _appDbContext.Remove(sa);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa thông tin Lương Tháng thành công." });
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var sa = await _appDbContext.SalaryOfMonth.FindAsync(Guid.Parse(Id));
            if (sa == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin Lương Tháng không tồn tại!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, sa);
            }
        }

        [HttpPost]
        [Route("getlist")]
        public response<ListSalary> GetList([FromBody] GridModel model)
        {
            var sa = _appDbContext.SalaryOfMonth.ToList();
            var users = _appDbContext.User.ToList();
            if (model.listFilter.Count != 0)
            {
                model.listFilter.ForEach(i =>
                {
                    if (!String.IsNullOrEmpty(i.filterDirections) && !String.IsNullOrEmpty(i.filterData))
                    {
                        switch (i.filterColumns)
                        {
                            case "FromUserId":
                                sa = _appDbContext.SalaryOfMonth.FromSqlRaw("SELECT * FROM public.\"SalaryOfMonth\" WHERE \"FromUserId\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "FuelAllowance":
                                sa = _appDbContext.SalaryOfMonth.FromSqlRaw("SELECT * FROM public.\"SalaryOfMonth\" WHERE \"FuelAllowance\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "LunchAllowance":
                                sa = _appDbContext.SalaryOfMonth.FromSqlRaw("SELECT * FROM public.\"SalaryOfMonth\" WHERE \"LunchAllowance\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "Money":
                                sa = _appDbContext.SalaryOfMonth.FromSqlRaw("SELECT * FROM public.\"SalaryOfMonth\" WHERE \"Money\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                        }
                    }
                });
            }
            if (!String.IsNullOrEmpty(model.searchText))
            {
                sa = (from s in sa
                     join u in users on s.FromUserId.ToString() equals u.Id
                     where u.FirstName.ToLower().Contains(model.searchText.ToLower()) || u.LastName.ToLower().Contains(model.searchText.ToLower())
                     select s).ToList();
                //sa = sa.Where(u => u.Note.Contains(model.searchText)).ToList();
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

        public class CreateSalary
        {
            public Guid FromUserId { get; set; }
            public string Money { get; set; }
            public string FuelAllowance { get; set; }
            public string LunchAllowance { get; set; }
        }

        public class UpdateSalary
        {
            public Guid Id { get; set; }
            public string Money { get; set; }
            public string FuelAllowance { get; set; }
            public string LunchAllowance { get; set; }
        }

        public class ListSalary
        {
            public Guid Id { get; set; }
            public Guid FromUserId { get; set; }
            public string Avatar { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Money { get; set; }
            public string FuelAllowance { get; set; }
            public string LunchAllowance { get; set; }
        }
    }
}
