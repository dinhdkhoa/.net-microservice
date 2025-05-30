using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Contracts;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Mappers;
using Play.Common;
using static Play.Catalog.Contracts.Contracts;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    [Authorize]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> repo;
        private readonly IPublishEndpoint publishEndpoint;
        private const string AdminRole = "Admin";

        // private static readonly List<ItemDto> items = new(){
        //     new ItemDto( "Potion", "Restores a small amount of HP", 5),
        //     new ItemDto( "Antidote", "Cures poison", 7),
        //     new ItemDto( "Bronze sword", "Deals a small amount of damage", 20)
        // };

        public ItemsController(IRepository<Item> _repo, IPublishEndpoint _publishEndpoint)
        {
            repo = _repo;
            publishEndpoint = _publishEndpoint;
        }

        [HttpGet]
        [Authorize(Policy = Policies.Read)]
        public async Task<IActionResult> Get()
        {
            var items = await repo.GetListAsync();
            return Ok(items.Select(item => item.ToItemDto()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await repo.GetByIdAsync(id);

            if(item == null){
                return NotFound();
            }

            return Ok(item.ToItemDto());
        }

        [HttpPost]
        [Authorize(Policy = Policies.Write)]
        public async Task<IActionResult> Add(CreateItemDto item)
        {
            if(!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var newItem = new ItemDto(item.Name, item.Description, item.Price);
            await repo.CreateAsync(newItem.ToItemFromDto());
            await publishEndpoint.Publish(new CatalogItemCreated(newItem.Id, newItem.Name, newItem.Description, newItem.Price));
            return CreatedAtAction(nameof(GetById), new { id = newItem.Id }, newItem);
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.Write)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await repo.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }
            await repo.DeleteAsync(id);
            await publishEndpoint.Publish(new CatalogItemDeleted(id));

            return NoContent();
        }
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.Write)]
        public async Task<IActionResult> Update(Guid id, UpdateItemDto req)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var item = await repo.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }
            
            item.Description = req.Description;
            item.Name = req.Name;
            item.Price = req.Price;

            await repo.UpdateAsync(item);
            await publishEndpoint.Publish(new CatalogItemUpdated(item.Id, item.Name, item.Description, item.Price));

            return NoContent();
        }
    }
}