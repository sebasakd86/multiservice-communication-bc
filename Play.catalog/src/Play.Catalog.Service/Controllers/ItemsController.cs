using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
//using Play.Catalog.Service.Models;

namespace Play.Catalog.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private static readonly List<ItemDto> items = new List<ItemDto>()
        {
            new ItemDto(Guid.NewGuid(), "Potion", "Restores a lil HP", 5, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "Antidote", "Cures Pablo", 7, DateTimeOffset.UtcNow),
            new ItemDto(Guid.NewGuid(), "Sword", "To chop enemies down", 20, DateTimeOffset.UtcNow)
        };
        public ItemsController(){}

        [HttpGet]
        public ActionResult<IEnumerable<ItemDto>> Get()
        {
            return Ok(items);
        }

        [HttpGet("{id}")]
        public ActionResult<ItemDto> GetById(Guid id)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if(item == null)
                return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public ActionResult<ItemDto> Post(CreateItemDto createItemDto)
        {
            var item = new ItemDto(Guid.NewGuid(), createItemDto.Name, createItemDto.Description, createItemDto.Price, DateTimeOffset.UtcNow);
            items.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id}, item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(Guid id, UpdateItemDto model)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if(item != null){
                var updatedItem = item with{
                    Name = model.Name,
                    Descripction = model.Description,
                    Price = model.Price
                };

                var ix = items.FindIndex(i => i.Id == id);
                items[ix] = updatedItem;
                return NoContent();
            }
            return NotFound();                
        }
        [HttpDelete("{id}")]
        public ActionResult<ItemDto> DeleteTModelById(Guid id)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if(item != null){
                var ix = items.FindIndex(i => i.Id == id);
                items.RemoveAt(ix);
                return Ok(item);
            }
            return NotFound();
        }
    }
}