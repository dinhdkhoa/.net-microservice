using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Play.Trading.Service.Controllers
{
    [ApiController]
    [Route("purchase")]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        public readonly IPublishEndpoint publishEndpoint;

        public PurchaseController(IPublishEndpoint publishEndpoint)
        {
            this.publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
        {
            var UserId = User.FindFirstValue("sub");
            var CorrelationId = Guid.NewGuid();

            var message = new PurchaseRequested(
                Guid.Parse(UserId),
                purchase.ItemId.Value,
                purchase.Quantity,
                CorrelationId);
            
            await publishEndpoint.Publish(message);

            return Accepted();
        }
    }
}