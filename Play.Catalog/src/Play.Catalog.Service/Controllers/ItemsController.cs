using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Mappers;
using Play.Common;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> repo;
        // private static readonly List<ItemDto> items = new(){
        //     new ItemDto( "Potion", "Restores a small amount of HP", 5),
        //     new ItemDto( "Antidote", "Cures poison", 7),
        //     new ItemDto( "Bronze sword", "Deals a small amount of damage", 20)
        // };

        public ItemsController(IRepository<Item> _repo)
        {
            repo = _repo;
        }

        [HttpGet]
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
        public async Task<IActionResult> Add(CreateItemDto item)
        {
            if(!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var newItem = new ItemDto(item.Name, item.Description, item.Price);
            await repo.CreateAsync(newItem.ToItemFromDto());
            return CreatedAtAction(nameof(GetById), new { id = newItem.Id }, newItem);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await repo.GetByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }
            await repo.DeleteAsync(id);
            return NoContent();
        }
        [HttpPut("{id}")]
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
            return NoContent();
        }
    }
}