using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static ManageUser.Controllers.DayOffController;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvanceMoneyController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;

        public AdvanceMoneyController(
            ApplicationDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateAdMoney model)
        {
            AdvanceMoney ad = new AdvanceMoney();
            ad.Id = Guid.NewGuid();
            ad.FromUserId = model.FromUserId;
            ad.Approval = "1";
            ad.ApprovelId = model.ApprovelId;
            ad.AdvanceDate = model.AdvanceDate;
            ad.Note = model.Note;
            ad.Money = model.Money;
            ad.CreateOn = DateTime.Now;
            ad.ModifyOn = DateTime.Now;
            await _appDbContext.AdvanceMoney.AddAsync(ad);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Đăng ký ứng tiền thành công." });
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UpdateAdMoney model)
        {
            var ad = await _appDbContext.AdvanceMoney.FindAsync(model.Id);
            if (ad == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin ứng tiền không tồn tại!" });
            }
            else
            {
                ad.FromUserId = model.FromUserId;
                ad.ApprovelId = model.ApprovelId;
                ad.AdvanceDate = model.AdvanceDate;
                ad.Note = model.Note;
                ad.Money = model.Money;
                ad.ModifyOn = DateTime.Now;
                _appDbContext.Update(ad);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin ứng tiền thành công." });
            }
        }

        [HttpPut]
        [Route("approval")]
        public async Task<IActionResult> Approval([FromBody] ApprovalModel model)
        {
            var ad = await _appDbContext.AdvanceMoney.FindAsync(model.Id);
            if (ad == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin ứng tiền không tồn tại!" });
            }

            ad.Approval = model.status;
            ad.ModifyOn = DateTime.Now;
            _appDbContext.Update(ad);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật trạng thái ứng tiền thành công." });
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var ad = await _appDbContext.AdvanceMoney.FindAsync(Guid.Parse(Id));
            if (ad == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin ứng tiền không tồn tại!" });
            }
            if (ad.Approval != "1")
            {
                return StatusCode(StatusCodes.Status409Conflict, new Response { Status = "Error", Message = "Thông tin ứng tiền ở trạng thái không được xóa!" });
            }
            // CHECK thêm mấy bảng bị phụ thuộc với advanceMoney
            _appDbContext.Remove(ad);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa thông tin ứng tiền thành công." });
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var ad = await _appDbContext.AdvanceMoney.FindAsync(Guid.Parse(Id));
            if (ad == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Thông tin ứng tiền không tồn tại!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, ad);
            }
        }

        [HttpPost]
        [Route("getlist")]
        public response<AdvanceMoney> GetList([FromBody] GridModel model)
        {
            var ad = _appDbContext.AdvanceMoney.ToList();
            if (model.listFilter.Count != 0)
            {
                model.listFilter.ForEach(i =>
                {
                    switch (i.filterColumns)
                    {
                        case "FromUserId":
                            ad = _appDbContext.AdvanceMoney.FromSqlRaw("SELECT * FROM public.\"AdvanceMoney\" WHERE \"FromUserId\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "Approval":
                            ad = _appDbContext.AdvanceMoney.FromSqlRaw("SELECT * FROM public.\"AdvanceMoney\" WHERE \"Approval\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "ApprovelId":
                            ad = _appDbContext.AdvanceMoney.FromSqlRaw("SELECT * FROM public.\"AdvanceMoney\" WHERE \"ApprovelId\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "AdvanceDate":
                            ad = _appDbContext.AdvanceMoney.FromSqlRaw("SELECT * FROM public.\"AdvanceMoney\" WHERE \"AdvanceDate\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "Money":
                            ad = _appDbContext.AdvanceMoney.FromSqlRaw("SELECT * FROM public.\"AdvanceMoney\" WHERE \"Money\"" + i.filterDirections + "'" + i.filterData + "'").ToList();
                            break;
                        case "FromDate":
                            ad = _appDbContext.AdvanceMoney.FromSqlRaw("SELECT * FROM public.\"AdvanceMoney\" WHERE \"AdvanceDate\" >= '" + i.filterData + "'").ToList();
                            break;
                        case "ToDate":
                            ad = _appDbContext.AdvanceMoney.FromSqlRaw("SELECT * FROM public.\"AdvanceMoney\" WHERE \"AdvanceDate\" <= '" + i.filterData + "'").ToList();
                            break;
                    }
                });
            }
            if (!String.IsNullOrEmpty(model.searchText))
            {
                ad = ad.Where(u => u.Note.Contains(model.searchText)).ToList();
            }
            if (!String.IsNullOrEmpty(model.srtColumns) && !String.IsNullOrEmpty(model.srtDirections))
            {
                switch (model.srtColumns)
                {
                    case "CreateOn":
                        if (model.srtDirections == "desc")
                        {
                            ad = ad.OrderByDescending(u => u.CreateOn).ToList();
                        }
                        else if (model.srtDirections == "asc")
                        {
                            ad = ad.OrderBy(u => u.CreateOn).ToList();
                        }
                        break;
                }
            }
            var data = ad;
            if (model.pageLoading)
            {
                ad = ad.Skip(model.pageSize * model.page).Take(model.pageSize).ToList();
            }

            response<AdvanceMoney> result = new response<AdvanceMoney>()
            {
                data = ad,
                dataCount = ad.Count(),
                page = model.page + 1,
                pageSize = model.pageSize,
                totalPages = Convert.ToInt32(Math.Ceiling(data.Count() / Convert.ToDouble(model.pageSize))),
                totalCount = data.Count()
            };
            return result;
        }

        public class CreateAdMoney
        {
            public Guid FromUserId { get; set; }
            public Guid ApprovelId { get; set; }
            public string Money { get; set; }
            public string Note { get; set; }
            public DateTime AdvanceDate { get; set; }
        }

        public class UpdateAdMoney
        {
            public Guid Id { get; set; }
            public Guid FromUserId { get; set; }
            public Guid ApprovelId { get; set; }
            public string Money { get; set; }
            public string Note { get; set; }
            public DateTime AdvanceDate { get; set; }
        }
    }
}
