using System;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Trading.Service.Entities;
using static Play.Catalog.Contracts.Contracts;

namespace Play.Trading.Service.Consumers
{
    public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
    {
        private readonly IRepository<CatalogItem> repo;

        public CatalogItemCreatedConsumer(IRepository<CatalogItem> _repo)
        {
            repo = _repo;
        }
        public async Task Consume(ConsumeContext<CatalogItemCreated> context)
        {
            var message = context.Message;
            var item = await repo.GetByIdAsync(message.Id);

            if(item != null){
                return ;
            }
            item = new CatalogItem
            {
                Id = message.Id,
                Description = message.description,
                Name = message.name,
                Price = message.price,
            };
            await repo.CreateAsync(item);
        }


    }
}