using System;
using System.Linq;
using System.Threading.Tasks;
using Authorization.Core.Models;
using Common.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;

namespace Authorization.SSO.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [LoggingFilter]
    [Route("sso/roles")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<Role> roleManager;

        public RolesController(
            RoleManager<Role> roleManager)
        {
            this.roleManager = roleManager;
        }


        [HttpGet("get")]
        public async Task<IActionResult> Get(long id) => Ok(await roleManager.FindByIdAsync(id.ToString()));


        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] Role model)
        {
            var role = await roleManager.FindByNameAsync(model.Name);
            if (role == null)
            {
                role = new Role(model);
                var result = await roleManager.CreateAsync(model);
                if (result.Errors.Any())
                    return BadRequest(result.Errors);
                role = await roleManager.FindByNameAsync(role.Name);
                return Ok(role);
            }
            return BadRequest();
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] Role model)
        {
            try
            {
                var role = await roleManager.FindByIdAsync(model.Id.ToString());
                role.NormalizedName = model.NormalizedName;
                role.AccountId = model.AccountId;
                role.CompanyId = model.CompanyId;
                role.ConcurrencyStamp = model.ConcurrencyStamp;
                role.Name = model.Name;

                var result = await roleManager.UpdateAsync(role);
                if (result.Errors.Any())
                    return Ok(false);

                return Ok(role);

            }
            catch (Exception e)
            {
                _ = e;
                return BadRequest();
            }
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var role = await roleManager.FindByIdAsync(id.ToString());
            var result = await roleManager.DeleteAsync(role);
            if (result.Errors.Any())
                return Ok(false);
            return Ok(true);
        }
    }
}
