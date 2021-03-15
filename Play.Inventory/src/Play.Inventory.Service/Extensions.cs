using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service
{
    public static class Extensions
    {
        public static InventoryItemDto AsDto(this InventoryItem item, string name, string description)
        {
            return new InventoryItemDto(
                item.CatalogItemId, 
                item.Quantity, 
                item.AcquiredDate,
                name,
                description
            );
        }
    }
}