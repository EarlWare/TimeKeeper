using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TimeKeeper.SharedLibrary.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TimeKeeperAPI.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IEnumerable<User>> Get()
        {
            var output = new List<User>();

            foreach (var user in _userManager.Users)
            {
                output.Add( await UserDtoFromIdentityUser(user) );
            }

            return output;
        }

        // GET api/Users/5
        [HttpGet("{id}")]
        public async Task<User> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var output = await UserDtoFromIdentityUser(user);
            return output;
        }

        // POST api/Users
        [HttpPost]
        public ActionResult Post([FromBody] string value)
        {
            return Problem("Method not yet implemented");
        }

        // PUT api/Users/5
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] string value)
        {
            return Problem("Method not yet implemented");
        }

        // DELETE api/Users/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            return Problem("Method not yet implemented");
        }


        [NonAction]
        private async Task<User> UserDtoFromIdentityUser(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var output = new User
            {
                UserID = user.Id,
                Username = user.UserName,
                Roles = roles
            };
            return output;
        }
    }
}
