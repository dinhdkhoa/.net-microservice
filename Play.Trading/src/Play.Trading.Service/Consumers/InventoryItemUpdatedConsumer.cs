using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Trading.Service.Entities;
using static Play.Inventory.Contracts.Contracts;

namespace Play.Trading.Service.Consumers
{
    public class InventoryItemUpdatedConsumer : IConsumer<InventoryItemUpdated>
    {
        private readonly IRepository<InventoryItem> repo;

        public InventoryItemUpdatedConsumer(IRepository<InventoryItem> repo)
        {
            this.repo = repo;
        }

        public async Task Consume(ConsumeContext<InventoryItemUpdated> context)
        {
            var message = context.Message;
            var item = await repo.GetAsync(
                item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId);

            if (item != null)
            {
                item.Quantity = message.NewTotalQuantity;
                await repo.UpdateAsync(item);
                return;
            }
            
            item = new InventoryItem
            {
                CatalogItemId = message.CatalogItemId,
                Quantity = message.NewTotalQuantity,
                UserId = message.UserId
            };
            await repo.CreateAsync(item);
        }
    }
}