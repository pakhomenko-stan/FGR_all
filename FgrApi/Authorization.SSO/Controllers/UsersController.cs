using System;
using System.Linq;
using System.Threading.Tasks;
using Authorization.Core.Models;
using Authorization.SSO.Extensions;
using Authorization.SSO.Filters;
using Common.Logging;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenIddict.Validation.AspNetCore;

namespace Authorization.SSO.Controllers
{
    [LoggingFilter]
    [Route("sso/users")]
    public class UsersController : ControllerBase
    {
        private readonly IEmailSender emailSender;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        private readonly ServerOptions options;
        private readonly ILogger<UsersController> logger;

        public UsersController(
            IEmailSender emailSender,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<ServerOptions> options,
            ILogger<UsersController> logger)
        {
            this.emailSender = emailSender;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.options = options.Value;
            this.logger = logger;
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpPost]
        [Route("add")]
        public async Task<IActionResult> Add([FromBody] UserModel model)
        {
            var entity = await userManager.FindByEmailAsync(model.Email);
            if (entity != null)
            {
                return ErrorResponse("User with same email is already exist");
            }
            model.IsInactive = false;

            entity = new User()
            {
                UserName = model.Username,
                FullName = model.FullName,
                CompanyId = model.CompanyId ?? -1,
                Email = model.Email,
                BusinessTitle = model.BusinessTitle,
                MobilePhone = model.MobilePhone,
                OfficePhone = model.OfficePhone,
                Department = model.Department,
                LastActivity = new DateTime(2000, 1, 1),
                IsInactive = model.IsInactive,
            };

            var result = await userManager.CreateAsync(entity);
            if (result.Errors.Any())
                return result.Errors.ErrorResponse();

            var user = await userManager.FindByNameAsync(entity.UserName);

            var passwordResult = await userManager.AddPasswordAsync(user, model.Password);
            if (result.Errors.Any())
                return result.Errors.ErrorResponse();

            var confirmToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailConfirmResult = await userManager.ConfirmEmailAsync(user, confirmToken);

            var roles = roleManager.Roles
                .Where(r => model.RoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToArray();

            result = await userManager.AddToRolesAsync(entity, roles);

            if (model.Notify ?? false)
            {
                await NotifyUserCreated(entity.Email, entity.FullName, entity.UserName, model.Password);
            }


            user.RoleIds = roleManager.Roles
                .Where(r => model.RoleIds.Contains(r.Id))
                .Select(r => r.Id);


            return Ok(JsonConvert.SerializeObject(user));
        }


        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpGet]
        [Route("get")]
        public async Task<IActionResult> Get(long id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null) ErrorResponse($"User with id={id} is not found");

            var userRoles = await userManager.GetRolesAsync(user);

            user.RoleIds = roleManager.Roles
                .Where(r => userRoles.Contains(r.Name) && r.CompanyId == user.CompanyId)
                .Select(r => r.Id);

            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpGet]
        [Route("delete/{soft}/{id}")]
        public async Task<IActionResult> Delete(string soft, long id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());

            if (user == null)
                throw new Exception($"User with id={id} is not found");

            user.IsInactive = true;
            var isSoft = soft == "soft";
            var result = isSoft
                ? await userManager.UpdateAsync(user)
                : await userManager.DeleteAsync(user);

            return result.Succeeded
                ? Ok(true)
                : Ok(false);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] UserModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id.ToString());
            var roles = await userManager.GetRolesAsync(user);

            if (user == null || user.IsInactive)
                return ErrorResponse($"User with id={model.Id} is not found");

            user.CustomSignature = model.CustomSignature;
            user.FullName = model.FullName;
            user.CompanyId = model.CompanyId ?? -1;
            user.Email = model.Email;
            user.BusinessTitle = model.BusinessTitle;
            user.Department = model.Department;
            user.MobilePhone = model.MobilePhone;
            user.OfficePhone = model.OfficePhone;

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded) return result.Errors.ErrorResponse();

            result = await userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded) return result.Errors.ErrorResponse();

            roles = roleManager.Roles
                .Where(r => model.RoleIds.Contains(r.Id))
                .Select(r => r.Name)
                .ToArray();

            result = await userManager.AddToRolesAsync(user, roles);

            user.RoleIds = roleManager.Roles
                .Where(r => model.RoleIds.Contains(r.Id))
                .Select(r => r.Id);

            return result.Succeeded
                ? Ok(user)
                : result.Errors.ErrorResponse();
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpPost]
        [Route("updatepassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] ResetPasswordModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId.ToString());

            if (user == null)
                return ErrorResponse($"User with id={model.UserId} is not found");

            IdentityResult result;

            if (string.IsNullOrEmpty(model.Password))
            {
                result = await userManager.RemovePasswordAsync(user);
                if (result.Succeeded) result = await userManager.AddPasswordAsync(user, model.NewPassword);
            }
            else
            {
                result = await userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            }

            return result.Succeeded
                ? Ok(true)
                : Ok(false);
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [ServiceFilter(typeof(RequestAddressFilter))]
        [HttpGet]
        [Route("validateusername")]
        public async Task<IActionResult> ValidateUsername(string username)
        {
            var user = await userManager.FindByNameAsync(username);
            return Ok(user == null);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsInactive)
            {
                return ErrorResponse($"User not found with email {model.Email}");
            }

            if (string.IsNullOrWhiteSpace(options.AppDomain)) return ErrorResponse("Configure app email settings. Please, contact to site administrator.");

            string code = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = string.Format("Please reset your password by clicking <a href='{0}'>here</a>", $"{options.AppDomain}/#/reset?token={code}");
            await emailSender.SendEmailAsync(user.Email, "Registration Success Message", callbackUrl);

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null || user.IsInactive) return ErrorResponse("User not found with this email");

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded) return result.Errors.ErrorResponse();
            return Ok();
        }

        private static IActionResult ErrorResponse(string message) => (new string[] { message }).ErrorResponse();

        [NonAction]
        private async Task NotifyUserCreated(string to, string name, string username, string password)
        {
            try
            {
                string siteUrl = options.AppDomain + "/#/login";
                string html = options.EMailMessage;

                string body = string.Format(html, name, siteUrl, username, password);
                await emailSender.SendEmailAsync(to, "Registration Success Message", body);
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex.Message);
            }
        }

    }
}
