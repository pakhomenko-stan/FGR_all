using Microsoft.AspNetCore.Mvc;
using CommonInterfaces.DTO;
using FGR.Application.Services;

namespace FGR.Api.Controllers
{
    //[Authorize]
    [Route("api/admin")]
    public class AdminController : BaseFgrController
    {

        [HttpGet("users/list")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetUserList([FromServices] UserActionsExecutor srv) => 
            await ActionAsync<UserActionsExecutor, UserActionsExecutor.Reply>(srv);

        [HttpGet("users/{id}")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetUser(int id, [FromServices] UserActionsExecutor srv) =>
            await ActionWithParameterAsync<int, UserActionsExecutor, UserActionsExecutor.Reply>(id, srv);
        
        [HttpGet("users/company/{company}")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetCompanyUsers(string company, [FromServices] UserActionsExecutor srv) =>
            await ActionWithParameterAsync<string, UserActionsExecutor, UserActionsExecutor.Reply>(company, srv);


        [HttpGet("users/{company}/v1/{id}")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> GetCompanyUser(string company, long id, [FromServices] UserActionsExecutor srv) =>
            await ActionWithParameterAsync<(string company, long id), UserActionsExecutor, UserActionsExecutor.Reply>((company, id), srv);


        [HttpPost("users/add")]
        public async Task<ActionResult<IWrapper<UserActionsExecutor.Reply?>>> AddUser([FromBody] UserActionsExecutor.Request user, [FromServices] UserActionsExecutor srv) => 
            await ActionAsync<UserActionsExecutor.Request, UserActionsExecutor, UserActionsExecutor.Reply>(srv, user);
    }
}
