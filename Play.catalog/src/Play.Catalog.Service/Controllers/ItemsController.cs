using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
//using Play.Catalog.Service.Models;

namespace Play.Catalog.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository _repo;
        public ItemsController(IRepository repo)
        {
            this._repo = repo;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> Get()
        {
            var items = await _repo.GetAll();
            return Ok(items.Select(i => i.AsDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetById(Guid id)
        {
            var item = await _repo.Get(id);
            if (item == null)
                return NotFound();
            return Ok(item.AsDto());
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
        {
            var item = new Item
            {   
                Name = createItemDto.Name, 
                Description = createItemDto.Description, 
                Price = createItemDto.Price, 
                CreatedDate = DateTimeOffset.UtcNow
            };
            await _repo.Create(item);            
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateItemDto model)
        {
            var item = await _repo.Get(id);
            if (item == null)
                return NotFound();

            item.Name = model.Name;
            item.Description = model.Description;
            item.Price = model.Price;

            await _repo.Update(id, item);

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ItemDto>> DeleteTModelById(Guid id)
        {
            var item = _repo.Get(id);
            if (item != null)
            {                
                await _repo.Remove(id);
                return NoContent();
            }
            return NotFound();
        }
    }
}