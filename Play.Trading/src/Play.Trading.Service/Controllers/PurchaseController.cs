using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Trading.Service.Contracts;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Controllers
{
    [ApiController]
    [Route("purchase")]
    [Authorize]
    public class PurchaseController : ControllerBase
    {
        public readonly IPublishEndpoint publishEndpoint;
        public readonly IRequestClient<GetPurchaseState> purchaseClient;

        public PurchaseController(IPublishEndpoint publishEndpoint, IRequestClient<GetPurchaseState> purchaseClient)
        {
            this.publishEndpoint = publishEndpoint;
            this.purchaseClient = purchaseClient;
        }

        [HttpGet("status/{correlationId}")]
        public async Task<IActionResult> GetStatusAsync(Guid correlationId)
        {
            var res = await purchaseClient.GetResponse<PurchaseState>(new GetPurchaseState(correlationId));
            var purchaseState = res.Message;
            var purchase = new PurchaseDto(
                purchaseState.UserId,
                purchaseState.ItemId,
                purchaseState.PurchaseTotal,
                purchaseState.Quantity,
                purchaseState.CurrentState,
                purchaseState.ErrorMessage,
                purchaseState.ReceivedAt,
                purchaseState.UpdatedAt
            );
            return Ok(purchase);
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

            return AcceptedAtAction(nameof(GetStatusAsync), new { CorrelationId }, new { CorrelationId });
        }
    }
}