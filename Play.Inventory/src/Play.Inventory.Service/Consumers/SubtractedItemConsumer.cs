using System;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;
using static Play.Inventory.Contracts.Contracts;

namespace Play.Inventory.Service.Consumers
{
    public class SubtractedItemConsumer : IConsumer<SubtractedItem>
    {
        private readonly IRepository<InventoryItem> inventoryRepo;

        private readonly IRepository<CatalogItem> catalogItemRepo;

        public SubtractedItemConsumer(IRepository<CatalogItem> catalogItemRepo, IRepository<InventoryItem> inventoryRepo)
        {
            this.catalogItemRepo = catalogItemRepo;
            this.inventoryRepo = inventoryRepo;
        }

        public async Task Consume(ConsumeContext<SubtractedItem> context)
        {
            var message = context.Message;
            
            var catalogItem = await catalogItemRepo.GetByIdAsync(message.CatalogItemId);
            if (catalogItem == null)
            {
                throw new UnknownItemException(message.CatalogItemId);
            }

            var currentItem = await inventoryRepo.GetAsync(item => item.UserId == message.UserId 
                                                            && item.CatalogItemId == message.CatalogItemId);

            if (currentItem != null)
            {
                currentItem.Quantity -= message.Quantity;
                await inventoryRepo.UpdateAsync(currentItem);

                if (currentItem.Quantity <= 0)
                {
                    await inventoryRepo.DeleteAsync(currentItem.Id);
                }
            }

            await context.Publish(new InventoryItemSubtracted(message.CorrelationId));
        }
    }
}
