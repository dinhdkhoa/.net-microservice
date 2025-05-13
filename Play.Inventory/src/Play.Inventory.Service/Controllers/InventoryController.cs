using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;
using static Play.Inventory.Contracts.Contracts;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class InventoryController : ControllerBase
    {
        private readonly IRepository<InventoryItem> repo;
        private const string AdminRole = "Admin";

        private readonly IRepository<CatalogItem> catalogItemRepo;
        private readonly IPublishEndpoint publishEndpoint;

        // private readonly CatalogClient catalogClient;

        public InventoryController(IRepository<InventoryItem> _repo, IRepository<CatalogItem> _catalogItemRepo, IPublishEndpoint publishEndpoint)
        {
            repo = _repo;
            catalogItemRepo = _catalogItemRepo;
            this.publishEndpoint = publishEndpoint;
            // catalogClient = _catalogClient;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }

            var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if(Guid.Parse(currentUserId) != userId && !User.IsInRole(AdminRole)) {
                return Forbid();    
            }

            // var catalogItems = await catalogClient.GetListCatalogAsync();

            var inventoryItemEntities = await repo.GetListAsync(item => item.UserId == userId);
            
            var catalogItems = await catalogItemRepo.GetListAsync(item => inventoryItemEntities.Select(item => item.CatalogItemId).Contains(item.Id));
            if (catalogItems.Count() == 0)
            {
                return Ok(catalogItems);
            }
            var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem => {
                var catalogItem = catalogItems.FirstOrDefault(c => c.Id == inventoryItem.CatalogItemId);
                return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
            });

            return Ok(inventoryItemDtos);
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> Create(GrantItemsDto req)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
            var currentItem = await repo.GetAsync(item => item.UserId == currentUserId && item.CatalogItemId == req.CatalogItemId);

            if(currentItem == null){
                var newItem = new InventoryItem{
                    AcquiredDate = DateTimeOffset.Now,
                    Quantity = req.Quantity,
                    UserId = currentUserId,
                    CatalogItemId = req.CatalogItemId
                };
                await repo.CreateAsync(newItem);
                // await catalogItemRepo.CreateAsync(new CatalogItem{
                //     Id =  req.CatalogItemId,
                //     Description = "Waiting To Be Updated From Catalog Service",
                //     Name = "Waiting To Be Updated From Catalog Service"
                // });
            }
            else {
                currentItem.Quantity += req.Quantity;
                await repo.UpdateAsync(currentItem);
            }

            await publishEndpoint.Publish(new InventoryItemUpdated(currentUserId, currentItem.CatalogItemId, currentItem.Quantity));

            return Ok();
        }
    }
}