using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        private static readonly List<ItemDto> items = new(){
            new ItemDto( "Potion", "Restores a small amount of HP", 5),
            new ItemDto( "Antidote", "Cures poison", 7),
            new ItemDto( "Bronze sword", "Deals a small amount of damage", 20)
        };

        [HttpGet]
        public IEnumerable<ItemDto> Get()
        {
            return items;
        }

        [HttpGet("{id}")]
        public ItemDto GetById(Guid id)
        {
            return items.FirstOrDefault(item => item.Id == id);
        }

        [HttpPost]
        public IActionResult Add(CreateItemDto item)
        {
            if(!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var newItem = new ItemDto(item.Name, item.Description, item.Price);
            items.Add(newItem);
            return CreatedAtAction(nameof(GetById), new { id = newItem.Id }, newItem);
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var item = items.FirstOrDefault(item => item.Id == id);
            if(item == null)
            {
                return NotFound();
            }
            items.Remove(item);
            return NoContent();
        }
        [HttpPut("{id}")]
        public IActionResult Update(Guid id, UpdateItemDto item)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var found = items.FindIndex(i => i.Id == id);
            if(found == -1)
            {
                return NotFound();
            }
            items[found] = items[found] with
            {
                Name = item.Name,
                Description = item.Description,
                Price = item.Price
            };
            return NoContent();
        }
    }
}