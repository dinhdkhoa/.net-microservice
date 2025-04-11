using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Play.Inventory.Contracts
{
    public class Contracts
    {
        public record GrantedItem(Guid UserId, Guid CatalogItemId, int Quantity, Guid CorrelationId);
        public record InventoryItemGranted( Guid CorrelationId);

        public record SubtractedItem(Guid UserId, Guid CatalogItemId, int Quantity, Guid CorrelationId);
        public record InventoryItemSubtracted(Guid CorrelationId);
    }
}