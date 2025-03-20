using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class InventoryController : ControllerBase
    {
        private readonly IRepository<InventoryItem> repo;
        private readonly IRepository<CatalogItem> catalogItemRepo;
        private readonly CatalogClient catalogClient;
        
        public InventoryController(IRepository<InventoryItem> _repo, IRepository<CatalogItem> _catalogItemRepo, CatalogClient _catalogClient)
        {
            repo = _repo;
            catalogItemRepo = _catalogItemRepo;
            catalogClient = _catalogClient;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest();
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
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> Create(GrantItemsDto req)
        {
            if (req.UserId == Guid.Empty)
            {
                return BadRequest();
            }

            var currentItem = await repo.GetAsync(item => item.UserId == req.UserId && item.CatalogItemId == req.CatalogItemId);

            if(currentItem == null){
                var newItem = new InventoryItem{
                    AcquiredDate = DateTimeOffset.Now,
                    Quantity = req.Quantity,
                    UserId = req.UserId,
                    CatalogItemId = req.CatalogItemId
                };
                await repo.CreateAsync(newItem);
            }
            else {
                currentItem.Quantity += req.Quantity;
                await repo.UpdateAsync(currentItem);
            }

            return Ok();
        }
    }
}