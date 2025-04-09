using System;

namespace Play.Inventory.Service.Dtos
{
    public record GrantItemsDto(Guid CatalogItemId, int Quantity);

    public record InventoryItemDto(Guid CatalogItemId, int Quantity,string name, string Description, DateTimeOffset AcquiredDate);

    public record CatalogItemDto(Guid Id, string Name, string Description);
}