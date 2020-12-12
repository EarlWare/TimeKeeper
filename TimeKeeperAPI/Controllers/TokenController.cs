using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TimeKeeperAPI.AuthData;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace TimeKeeperAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        //private readonly AuthDBContext _dBContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public TokenController(UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;

            EnsureSetup();
        }

        // POST api/Token
        [HttpPost(Name = nameof(Token))]
        public async Task<IActionResult> Token(string username, string password)
        {
            var user = await AuthorizedUser(username, password);
            if (user != null)
            {
                return new ObjectResult(await CreateTokenFor(user));
            }
            else
            {
                return BadRequest();
            }
        }


        /// <summary>
        /// Finds an authorized user based on the username and password strings passed in.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>A valid user if one is found, or null</returns>
        [NonAction]
        private async Task<IdentityUser> AuthorizedUser(string username, string password)
        {
            var user = await _userManager.FindByEmailAsync(username);

            if (await _userManager.CheckPasswordAsync(user, password))
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates the encrypted Json Web Token for the user passed in. 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A valid JWT</returns>
        [NonAction]
        private async Task<string> CreateTokenFor(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.Now.AddDays(5).ToUnixTimeSeconds().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(new SymmetricSecurityKey(Encoding.Unicode.GetBytes(_config["SigningKey"])),
                                                         SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));

            var output = new JwtSecurityTokenHandler().WriteToken(token);

            return output;
        }


        // ensures the auth db is initally setup.
        [NonAction]
        private void EnsureSetup()
        {
            // ensure all user roles are setup.
            EnsureRoles();

            // ensure the master acct is created.
            EnsureMasterCreated();

            // ensure the master account is setup
            EnsureMasterSetup();
        }

        private async void EnsureRoles()
        {
            var roles = new[]
            {
                AccountRoles.Admin,
                AccountRoles.Manager,
                AccountRoles.Standard
            };

            // ensure each role in our standard list is in the authDB
            foreach (var role in roles)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    continue;
                }
                else
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async void EnsureMasterCreated()
        {
            // ensure the master acct is created.
            var user = await _userManager.FindByEmailAsync(_config["MasterAcct:Email"]);
            if (await _userManager.FindByEmailAsync(_config["MasterAcct:Email"]) == null)
            {
                var result = await _userManager.CreateAsync(new IdentityUser(_config["MasterAcct:Email"])
                {
                    Email = _config["MasterAcct:Email"]
                });
                return;
            }
            return;
        }

        private async void EnsureMasterSetup()
        {
            var user = await _userManager.FindByEmailAsync(_config["MasterAcct:Email"]);
            var hasPassword = await _userManager.HasPasswordAsync(user);
            var isAdmin = await _userManager.IsInRoleAsync(user, AccountRoles.Admin);
            var isManager = await _userManager.IsInRoleAsync(user, AccountRoles.Manager);
            if (hasPassword == false)  // needs a password
            {
                await _userManager.AddPasswordAsync(user, _config["MasterAcct:Password"]);
            }
            if (isAdmin == false)  // need to add admin role
            {
                await _userManager.AddToRoleAsync(user, AccountRoles.Admin);
            }
            if (isManager == false) //  need to add manager role
            {
                await _userManager.AddToRoleAsync(user, AccountRoles.Manager);
            }
            return;
        }
    }

    internal static class AccountRoles
    {
        public static string Admin => "Admin";
        public static string Manager => "Manager";
        public static string Standard => "Standard";
    }
}
