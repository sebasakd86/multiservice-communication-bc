using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class ItemsController : ControllerBase
    {
        private readonly IRepository<InventoryItem> _itemRepository;
        private readonly CatalogClient _catalogClient;
        public ItemsController(IRepository<InventoryItem> itemRepository, CatalogClient catalogClient)
        {
            this._catalogClient = catalogClient;
            this._itemRepository = itemRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();
            // var items = (await _itemRepository.GetAll(item => item.UserId == userId))
            //             .Select(item => item.AsDto());
            var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            var inventoryItemsEntities = await _itemRepository.GetAll(item => item.UserId == userId);
            var inventoryItemDtos = inventoryItemsEntities.Select(i => {
                var catalogItem = catalogItems.SingleOrDefault(ci => ci.Id == i.CatalogItemId);
                return i.AsDto(catalogItem.Name, catalogItem.Descripction);
            });
            return Ok(inventoryItemDtos);
        }

        [HttpPost()]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await _itemRepository.Get(
                item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

            if (inventoryItem == null)
            {
                inventoryItem = new InventoryItem
                {
                    CatalogItemId = grantItemsDto.CatalogItemId,
                    UserId = grantItemsDto.UserId,
                    Quantity = grantItemsDto.Quantity,
                    AcquiredDate = DateTimeOffset.UtcNow
                };
                await _itemRepository.Create(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await _itemRepository.Update(inventoryItem.Id, inventoryItem);
            }
            return Ok();
        }
    }
}