using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Play.Common;
using Play.Trading.Service.Entities;
using static Play.Identity.Contracts.Contracts;

namespace Play.Trading.Service.Consumers
{
    public class UserUpdatedConsumer : IConsumer<UserUpdated>
    {
        private readonly IRepository<ApplicationUser> repo;

        public UserUpdatedConsumer(IRepository<ApplicationUser> repo)
        {
            this.repo = repo;
        }

        public async Task Consume(ConsumeContext<UserUpdated> context)
        {
            var message = context.Message;
            var user = await repo.GetByIdAsync(message.UserId);

            if(user == null){
                user = new ApplicationUser
                {
                    Id = message.UserId,
                    Gil = message.NewTotalQuantity
                };
                await repo.CreateAsync(user);
                return;
            }

            user.Gil = message.NewTotalQuantity;
            await repo.UpdateAsync(user);
        }
    }
}