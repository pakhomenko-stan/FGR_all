using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CommonInterfaces.DTO;
using FGR.Application.Services;
using Authorization.Lib.Helpers;

namespace FGR.Api.Controllers
{
    [Authorize(Policy = FgrTermsHelper.AdminUIPolicy)]
    [Route("api/admin")]
    public class AdminController : BaseFgrController
    {

        [HttpGet("users/list")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetUserList([FromServices] UserActionsExecutor srv, CancellationToken token) => 
            await ActionAsync<UserActionsExecutor, UserActionsExecutor.Reply>(srv, token);

        [HttpGet("users/{id}")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetUser(int id, [FromServices] UserActionsExecutor srv, CancellationToken token) =>
            await ActionWithParameterAsync<int, UserActionsExecutor, UserActionsExecutor.Reply>(id, srv, token);
        
        [HttpGet("users/company/{company}")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetCompanyUsers(string company, [FromServices] UserActionsExecutor srv, CancellationToken token) =>
            await ActionWithParameterAsync<string, UserActionsExecutor, UserActionsExecutor.Reply>(company, srv, token);


        [HttpGet("users/{company}/v1/{id}")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetCompanyUser(string company, long id, [FromServices] UserActionsExecutor srv, CancellationToken token) =>
            await ActionWithParameterAsync<(string company, long id), UserActionsExecutor, UserActionsExecutor.Reply>((company, id), srv, token);


        [HttpPost("users/add")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> AddUser([FromBody] UserActionsExecutor.Request user, [FromServices] UserActionsExecutor srv, CancellationToken token) => 
            await ActionAsync<UserActionsExecutor.Request, UserActionsExecutor, UserActionsExecutor.Reply>(srv, user, token);
    }
}
