using System;

namespace Play.Catalog.Contracts
{
    //To send to the message broker
    public record CatalogItemCreated(
        Guid ItemId,
        string Name,
        string Description
    ); 
    public record CatalogItemUpdated(
        Guid ItemId,
        string Name,
        string Description
    ); 
    public record CatalogItemDeleted(Guid ItemId);
}