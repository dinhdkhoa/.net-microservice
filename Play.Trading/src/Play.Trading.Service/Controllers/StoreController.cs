using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Controllers
{
    [ApiController]
    [Route("store")]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IRepository<CatalogItem> catalogRepo;
        private readonly IRepository<ApplicationUser> userRepo;
        private readonly IRepository<InventoryItem> inventoryRepo;
        public StoreController(IRepository<CatalogItem> catalogRepo, IRepository<InventoryItem> inventoryRepo, IRepository<ApplicationUser> userRepo)
        {
            this.catalogRepo = catalogRepo;
            this.inventoryRepo = inventoryRepo;
            this.userRepo = userRepo;
        }

        [HttpGet]
        public async Task<ActionResult<StoreDto>> GetAsync()
        {
            var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));

            var user = await userRepo.GetByIdAsync(userId);
            if(user == null) return NotFound("User Info Not Found");

            // var catalogitems = await catalogRepo.GetListAsync();
            var inventoryItems = await inventoryRepo.GetListAsync(item => item.UserId == userId);

            if(inventoryItems.Count == 0) {
                return Ok(new StoreDto(new List<StoreItemDto>() , user.Gil));
            }

            var catalogItemIds = inventoryItems.Select(i => i.CatalogItemId).Distinct().ToList();
            var catalogItems = await catalogRepo.GetListAsync(c => catalogItemIds.Contains(c.Id));

            var storeItems = catalogItems.Select(item => new StoreItemDto(
                item.Id,
                item.Name,
                item.Description,
                item.Price,
                inventoryItems.FirstOrDefault(inventoryItem => inventoryItem.CatalogItemId == item.Id)?.Quantity ?? 0
            ));

            return Ok(new StoreDto(storeItems, user.Gil));
        }
    }
}