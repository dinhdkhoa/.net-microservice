using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Catalog.Contracts.Contracts;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
    {
        private readonly IRepository<CatalogItem> repo;

        public CatalogItemUpdatedConsumer(IRepository<CatalogItem> _repo)
        {
            repo = _repo;
        }
        public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
        {
            var message = context.Message;
            var item = await repo.GetByIdAsync(message.Id);

            if (item == null)
            {
                item = new CatalogItem
                {
                    Id = message.Id,
                    Description = message.description,
                    Name = message.name
                };
                await repo.CreateAsync(item);
            } else {
                item.Name = message.name;
                item.Description = message.description;

                await repo.UpdateAsync(item);
            }

            
        }

    }
}