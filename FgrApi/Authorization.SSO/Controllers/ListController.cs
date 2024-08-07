using System;
using System.Collections.Generic;
using Authorization.SSO.Extensions;
using Authorization.SSO.Filters;
using Authorization.SSO.Providers;
using Common.Interfaces;
using Common.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;

namespace Authorization.SSO.Controllers
{
    [LoggingFilter]
    public class ListController : ControllerBase
    {
        private readonly ListProvider listProvider;

        public ListController(ListProvider listProvider)
        {
            this.listProvider = listProvider;
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpGet]
        [Route("sso/roles/list")]
        public IActionResult RoleList() => new OkObjectResult(listProvider.GetRoles);

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpGet]
        [Route("sso/users/list")]
        public IActionResult UserList()
        {
            try
            {
                var usersWithRoles = listProvider.GetUsers;
                var list = JsonConvert.SerializeObject(
                    usersWithRoles,
                    typeof(IEnumerable<IUser>),
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                return Ok(list);
            }
            catch (Exception e)
            {
                var msg = e.Message;
                return ErrorResponse(msg);
            }
        }

        private static IActionResult ErrorResponse(string message) => (new string[] { message }).ErrorResponse();
    }
}
