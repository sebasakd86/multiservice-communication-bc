using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;
//using Play.Catalog.Service.Models;

namespace Play.Catalog.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        public ItemsController(IRepository<Item> repo, IPublishEndpoint publishEndpoint)
        {
            this._publishEndpoint = publishEndpoint;
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
            //Publish a message to the MQ
            await _publishEndpoint.Publish(new CatalogItemCreated(
                item.Id, item.Name, item.Description
            ));
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
            //Publish a message to the MQ
            await _publishEndpoint.Publish(new CatalogItemUpdated(
                id, item.Name, item.Description
            ));
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ItemDto>> DeleteTModelById(Guid id)
        {
            var item = _repo.Get(id);
            if (item != null)
            {
                await _repo.Remove(id);
                //Publish a message to the MQ
                await _publishEndpoint.Publish(new CatalogItemDeleted(id));                
                return NoContent();
            }
            return NotFound();
        }
    }
}