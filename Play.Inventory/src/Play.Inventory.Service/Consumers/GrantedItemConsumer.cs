using System;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using Play.Inventory.Service.Exceptions;
using static Play.Inventory.Contracts.Contracts;

namespace Play.Inventory.Service.Consumers
{
    public class GrantedItemConsumer : IConsumer<GrantedItem>
    {
        private static int retry = 0;
        private readonly IRepository<InventoryItem> inventoryRepo;

        private readonly IRepository<CatalogItem> catalogItemRepo;

        public GrantedItemConsumer(IRepository<CatalogItem> catalogItemRepo, IRepository<InventoryItem> inventoryRepo)
        {
            this.catalogItemRepo = catalogItemRepo;
            this.inventoryRepo = inventoryRepo;
        }

        public async Task Consume(ConsumeContext<GrantedItem> context)
        {
            var message = context.Message;

            var catalogItem = await catalogItemRepo.GetByIdAsync(message.CatalogItemId);
            if(catalogItem == null){
                throw new UnknownItemException(message.CatalogItemId);
            }

            var currentItem = await inventoryRepo.GetAsync(item => item.UserId == message.UserId
                                                && item.CatalogItemId == message.CatalogItemId);

            if (currentItem == null)
            {
                currentItem = new InventoryItem
                {
                    AcquiredDate = DateTimeOffset.Now,
                    Quantity = message.Quantity,
                    UserId = message.UserId,
                    CatalogItemId = message.CatalogItemId,
                };
                currentItem.MesssageIds.Add(context.MessageId.Value);
                await inventoryRepo.CreateAsync(currentItem);
            }
            else
            {   
                if(currentItem.MesssageIds.Contains(context.MessageId.Value)){
                    await context.Publish(new InventoryItemGranted(message.CorrelationId));
                    return;
                }
                currentItem.Quantity += message.Quantity;
                await inventoryRepo.UpdateAsync(currentItem);
            }

            var inventoryItemGrantedTask = context.Publish(new InventoryItemGranted(message.CorrelationId));
            var inventoryItemUpdated = context.Publish(new InventoryItemUpdated(message.UserId, message.CatalogItemId, currentItem.Quantity));

            await Task.WhenAll(inventoryItemGrantedTask, inventoryItemUpdated);
        }
    }
}
