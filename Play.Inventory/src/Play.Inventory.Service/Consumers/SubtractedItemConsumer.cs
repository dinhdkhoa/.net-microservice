using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.VisualBasic;
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


            if (currentItem != null && !currentItem.MesssageIds.Contains(context.MessageId.Value))
            {
                currentItem.Quantity -= message.Quantity;
                currentItem.MesssageIds.Add(context.MessageId.Value);
                await inventoryRepo.UpdateAsync(currentItem);
                await context.Publish(new InventoryItemUpdated(message.UserId, message.CatalogItemId, message.Quantity));

                if (currentItem.Quantity <= 0)
                {
                    await inventoryRepo.DeleteAsync(currentItem.Id);
                }
            }

            await context.Publish(new InventoryItemSubtracted(message.CorrelationId));
        }
    }
}
