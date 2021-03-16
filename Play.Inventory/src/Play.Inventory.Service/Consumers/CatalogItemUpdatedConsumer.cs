using System.Threading.Tasks;
using MassTransit;
using Play.Catalog.Contracts;
using Play.Common;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> repository;
        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> repository)
        {
            this.repository = repository;
        }
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;
            var item = await repository.Get(message.ItemId);
            //Already created                
            if(item == null) 
            {
                item = new CatalogItem{
                    Id = message.ItemId,
                    Name = message.Name,
                    Description = message.Description
                };
                await repository.Create(item);
            }
            else 
            {
                item.Name = message.Name;
                item.Description = message.Description;
                await repository.Update(message.ItemId, item);
            }
        }
    }
}