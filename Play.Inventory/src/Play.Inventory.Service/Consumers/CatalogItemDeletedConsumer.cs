using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Catalog.Contracts.Contracts;

namespace Play.Inventory.Service.Consumers
{
    public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
    {
        private readonly IRepository<CatalogItem> repo;

        public CatalogItemDeletedConsumer(IRepository<CatalogItem> _repo)
        {
            repo = _repo;
        }
        public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
        {
            var message = context.Message;
            var item = await repo.GetByIdAsync(message.Id);

            if (item == null)
            {
                return;
            }

            await repo.DeleteAsync(item.Id);
        }
    }
}