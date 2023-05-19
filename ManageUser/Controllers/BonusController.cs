using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BonusController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;

        public BonusController(
            ApplicationDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateBonus model)
        {
            Bonus bonus = new Bonus();
            bonus.Id = Guid.NewGuid();
            bonus.FromUserId = model.FromUserId;
            bonus.Description = model.Description;
            bonus.DateBonus = model.DateBonus;
            bonus.Money = model.Money;
            bonus.CreateOn = DateTime.Now;
            bonus.ModifyOn = DateTime.Now;
            await _appDbContext.Bonus.AddAsync(bonus);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Thêm mới Bonus thành công." });
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateBonus model)
        {
            var bonus = await _appDbContext.Bonus.FindAsync(model.Id);
            if (bonus == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin Bonus không tồn tại!" });
            }
            else
            {
                bonus.FromUserId = model.FromUserId;
                bonus.Description = model.Description;
                bonus.DateBonus= model.DateBonus;
                bonus.Money = model.Money;
                bonus.ModifyOn = DateTime.Now;
                _appDbContext.Update(bonus);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin bonus thành công." });
            }
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var bonus = await _appDbContext.Bonus.FindAsync(Guid.Parse(Id));
            if (bonus == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Bonus không tồn tại!" });
            }
            else
            {
                // CHECK thêm mấy bảng bị phụ thuộc với bonus
                _appDbContext.Remove(bonus);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa thông tin bonus thành công." });
            }
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var bonus = await _appDbContext.Bonus.FindAsync(Guid.Parse(Id));
            if (bonus == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Bonus không tồn tại!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, bonus);
            }
        }

        [HttpPost]
        [Route("getlist")]
        public response<Bonus> GetList([FromBody] GridModel model)
        {
            var bonus = _appDbContext.Bonus.ToList();
            if (model.listFilter.Count != 0)
            {
                model.listFilter.ForEach(i =>
                {
                    if (!String.IsNullOrEmpty(i.filterDirections) && !String.IsNullOrEmpty(i.filterData))
                    {
                        switch (i.filterColumns)
                        {
                            case "FromUserId":
                                bonus = _appDbContext.Bonus.FromSqlRaw("SELECT * FROM public.\"Bonus\" WHERE \"FromUserId\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "DateBonus":
                                bonus = _appDbContext.Bonus.FromSqlRaw("SELECT * FROM public.\"Bonus\" WHERE \"DateBonus\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "Money":
                                bonus = _appDbContext.Bonus.FromSqlRaw("SELECT * FROM public.\"Bonus\" WHERE \"Money\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                                break;
                            case "FromDate":
                                bonus = _appDbContext.Bonus.FromSqlRaw("SELECT * FROM public.\"Bonus\" WHERE \"DateBonus\" >= '" + i.filterData + "'").ToList();
                                break;
                            case "ToDate":
                                bonus = _appDbContext.Bonus.FromSqlRaw("SELECT * FROM public.\"Bonus\" WHERE \"DateBonus\" <= '" + i.filterData + "'").ToList();
                                break;
                        }
                    } 
                });
            }
            if (!String.IsNullOrEmpty(model.searchText))
            {
                bonus = bonus.Where(u => u.Description.ToLower().Contains(model.searchText.ToLower())).ToList();
            }
            if (!String.IsNullOrEmpty(model.srtColumns) && !String.IsNullOrEmpty(model.srtDirections))
            {
                switch (model.srtColumns)
                {
                    case "CreateOn":
                        if (model.srtDirections == "desc")
                        {
                            bonus = bonus.OrderByDescending(u => u.CreateOn).ToList();
                        }
                        else if (model.srtDirections == "asc")
                        {
                            bonus = bonus.OrderBy(u => u.CreateOn).ToList();
                        }
                        break;
                }
            }
            var data = bonus;
            if (model.pageLoading)
            {
                bonus = bonus.Skip(model.pageSize * model.page).Take(model.pageSize).ToList();
            }

            response<Bonus> result = new response<Bonus>()
            {
                data = bonus,
                dataCount = bonus.Count(),
                page = model.page + 1,
                pageSize = model.pageSize,
                totalPages = Convert.ToInt32(Math.Ceiling(data.Count() / Convert.ToDouble(model.pageSize))),
                totalCount = data.Count()
            };
            return result;
        }

        public class CreateBonus
        {
            public Guid FromUserId { get; set; }
            public string Description { get; set; }
            public DateTime DateBonus { get; set; }
            public string Money { get; set; }
        }

        public class UpdateBonus
        {
            public Guid Id { get; set; }
            public Guid FromUserId { get; set; }
            public string Description { get; set; }
            public DateTime DateBonus { get; set; }
            public string Money { get; set; }
        }
    }
}
