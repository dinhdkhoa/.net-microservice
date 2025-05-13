using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Play.Identity.Service.Dtos;
using Play.Identity.Service.Entities;
using static Duende.IdentityServer.IdentityServerConstants;
using static Play.Identity.Contracts.Contracts;

namespace Play.Identity.Service.Controllers
{
    [ApiController]
    [Route("users")]
    [Authorize(Policy = LocalApi.PolicyName, Roles = AdminRole)]
    public class UsersControllers : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPublishEndpoint publishEndpoint;

        private const string AdminRole = "Admin";

        public UsersControllers(UserManager<ApplicationUser> _userManager, IPublishEndpoint publishEndpoint)
        {
            userManager = _userManager;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public ActionResult<IEnumerable<UserDto>> Get() {
            var users = userManager.Users.ToList().Select(user => user.AsDto());
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetByIdAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if(user == null) return NotFound();
            return user.AsDto();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateAsync(Guid id, UpdateUserDto req)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            user.Email = req.Email;
            user.Gil = req.Gil;
            await userManager.UpdateAsync(user);
            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, user.Gil));
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserDto>> DeleteAsync(Guid id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            await userManager.DeleteAsync(user);
            await publishEndpoint.Publish(new UserUpdated(user.Id, user.Email, 0));
            return NoContent();
        }
    }
}