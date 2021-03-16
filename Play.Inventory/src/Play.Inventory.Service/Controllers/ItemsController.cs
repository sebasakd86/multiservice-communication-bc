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
        private readonly IRepository<InventoryItem> inventoryItemRepository;
        private readonly IRepository<CatalogItem> catalogRepository;
        public ItemsController(IRepository<InventoryItem> itemRepository, IRepository<CatalogItem> catalogRepository)
        {
            this.catalogRepository = catalogRepository;
            this.inventoryItemRepository = itemRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();
            // var items = (await _itemRepository.GetAll(item => item.UserId == userId))
            //             .Select(item => item.AsDto());
            // var catalogItems = await _catalogClient.GetCatalogItemsAsync();
            var items = (await inventoryItemRepository.GetAll(item => item.UserId == userId));
            var itemIds = items.Select(i => i.CatalogItemId);
            var catalogItemsEntities = await catalogRepository.GetAll(i => itemIds.Contains(i.Id));

            var inventoryItemsEntities = await inventoryItemRepository.GetAll(item => item.UserId == userId);
            var inventoryItemDtos = inventoryItemsEntities.Select(i =>
            {
                var catalogItem = catalogItemsEntities.SingleOrDefault(ci => ci.Id == i.CatalogItemId);
                return i.AsDto(catalogItem.Name, catalogItem.Description);
            });
            return Ok(inventoryItemDtos);
        }

        [HttpPost()]
        public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
        {
            var inventoryItem = await inventoryItemRepository.Get(
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
                await inventoryItemRepository.Create(inventoryItem);
            }
            else
            {
                inventoryItem.Quantity += grantItemsDto.Quantity;
                await inventoryItemRepository.Update(inventoryItem.Id, inventoryItem);
            }
            return Ok();
        }
    }
}