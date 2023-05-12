using ManageUser.Authentication;
using ManageUser.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ManageUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionController : ControllerBase
    {
        private readonly ApplicationDbContext _appDbContext;

        public PositionController(
            ApplicationDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            Position position = new Position();
            position.Name = name;
            position.Id = Guid.NewGuid();
            await _appDbContext.Position.AddAsync(position);
            await _appDbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Thêm mới Chức Vụ thành công." });
        }

        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] Position model)
        {
            var position = await _appDbContext.Position.FindAsync(model.Id);
            if (position == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Chức Vụ không tồn tại!" });
            }
            else
            {
                position.Name = model.Name;
                _appDbContext.Update(position);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Cập nhật thông tin Chức Vụ thành công." });
            }
        }

        [HttpDelete]
        [Route("delete/{Id}")]
        public async Task<IActionResult> Delete(string Id)
        {
            var position = await _appDbContext.Position.FindAsync(Guid.Parse(Id));
            if (position == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Chức Vụ không tồn tại!" });
            }
            else
            {
                // CHECK thêm mấy bảng bị phụ thuộc với position
                _appDbContext.Remove(position);
                await _appDbContext.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Xóa thông tin Chức Vụ thành công." });
            }
        }

        [HttpGet]
        [Route("get/{Id}")]
        public async Task<IActionResult> GetOne(string Id)
        {
            var position = await _appDbContext.Position.FindAsync(Guid.Parse(Id));
            if (position == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new Response { Status = "Error", Message = "Chức Vụ không tồn tại!" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, position);
            }
        }

        [HttpPost]
        [Route("getlist")]
        public IEnumerable<Position> GetList([FromBody] GridModel model)
        {
            var position = _appDbContext.Position.ToList();
            var list = from po in position
                       select new Position()
                       {
                           Id = po.Id,
                           Name = po.Name
                       };

            return list;
        }
    }
}
