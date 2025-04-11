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
                var newItem = new InventoryItem
                {
                    AcquiredDate = DateTimeOffset.Now,
                    Quantity = message.Quantity,
                    UserId = message.UserId,
                    CatalogItemId = message.CatalogItemId
                };
                await inventoryRepo.CreateAsync(newItem);
            }
            else
            {
                currentItem.Quantity += message.Quantity;
                await inventoryRepo.UpdateAsync(currentItem);
            }

            await context.Publish(new InventoryItemGranted(message.CorrelationId));
        }
    }
}
