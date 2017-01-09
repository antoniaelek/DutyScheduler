using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DutyScheduler.Helpers;
using DutyScheduler.Models;
using DutyScheduler.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Swashbuckle.SwaggerGen.Annotations;

namespace DutyScheduler.Controllers 
{
    /// <summary>
    /// User resource.
    /// </summary>
    [Route("api/[controller]/")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserController(ApplicationDbContext context,
            UserManager<User> userManager,
            IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Build();
        }

        /// <summary>
        /// Get user with the specified <paramref name="username"/>.
        /// </summary>
        /// <param name="username">Unique identifier of the user.</param>
        /// <returns>JSON user data.</returns>
        [HttpGet("{username}")]
        [SwaggerResponse(HttpStatusCode.OK, "Users successfully fetched.")]
        [AllowAnonymous]
        public async Task<JsonResult> Get(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username.ToUpper());
            if (user == null) return 404.ErrorStatusCode();
            return user.ToJson();
        }

        /// <summary>
        /// Get all users.
        /// </summary>
        /// <returns>Users.</returns>
        [SwaggerResponse(HttpStatusCode.OK, "Users successfully fetched.")]
        [HttpGet]
        [AllowAnonymous]
        public JsonResult Get()
        {
            _context.Users.Load();
            var users = _context.Users.ToList();
            return users.ToJson();
        }

        /// <summary>
        /// Register.
        /// </summary>
        /// <param name="viewModel">User profile to be created.</param>
        /// <returns>New user JSON data.</returns>
        [SwaggerResponse(HttpStatusCode.Created, "User profile successfully created.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Validation errors.")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Create([FromBody]RegisterViewModel viewModel)
        {
            _context.Users.Load();
            var usernames = _context.Users.Select(u => u.NormalizedUserName);
            if (usernames.Contains(viewModel.UserName.ToUpper()))
                ModelState.AddModelError("UserName", "The specified username is taken.");

            var emails = _context.Users.Select(u => u.NormalizedEmail);
            if (emails.Contains(viewModel.Email.ToUpper()))
                ModelState.AddModelError("Email", "There already exists an account registered with the specified email.");

            if (viewModel.Password.Length < 4)
                ModelState.AddModelError("Password", "Password must contain at least 4 characters.");

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = viewModel.UserName,
                    Email = viewModel.Email,
                    Name = viewModel.Name,
                    LastName = viewModel.LastName,
                    Office = viewModel.Office,
                    Phone = viewModel.Phone
                };

                
                var result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    return user.ToJson(201);
                }
            
            }
            if (!ModelState.Keys.Any()) 
                ModelState.AddModelError("Email","There already exists an account with that email.");
            var allErrors = ModelState.ValidationErrors();
            var ret = new JsonResult(new { Errors = allErrors});
            ret.StatusCode = 400;
            return ret;
        }

        /// <summary>
        /// Update user with the specified <paramref name="username"/>.
        /// </summary>
        /// <param name="username">Unique identifier of the user.</param>
        /// <param name="viewModel">New user data.</param>
        /// <returns>JSON user data.</returns>
        [SwaggerResponse(HttpStatusCode.OK, "User details successfully saved.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [HttpPut("{username}")]
        [Authorize]
        public async Task<JsonResult> Update(string username, [FromBody]UpdateUserViewModel viewModel)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username.ToUpper());
            if (user == null) return 404.ErrorStatusCode();

            var isAuthorized = await CheckUserCredentials(user.UserName);
            if (isAuthorized.StatusCode != 200 && isAuthorized.StatusCode.HasValue)
                return isAuthorized.StatusCode.Value.ErrorStatusCode();

            if (isAuthorized.StatusCode != 200)
                return 400.ErrorStatusCode();

            if (viewModel.Name != null) user.Name = viewModel.Name;
            if (viewModel.LastName != null) user.LastName = viewModel.LastName;
            if (viewModel.Office != null) user.Office = viewModel.Office;
            if (viewModel.Phone != null) user.Phone = viewModel.Phone;

            _context.Users.Update(user);
            _context.SaveChanges();

            return user.ToJson();
        }

        /// <summary>
        /// Grant to or revoke admin rigts from user with the specified <paramref name="username"/>.
        /// </summary>
        /// <param name="username">Unique identifier of the user.</param>
        /// <param name="viewModel">Updated user data.</param>
        /// <returns>JSON user data.</returns>
        [SwaggerResponse(HttpStatusCode.OK, "User admin rights successfully updated.")]
        [SwaggerResponse(HttpStatusCode.NotModified, "Model was empty, nothing happened.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [HttpPut("admin/{username}")]
        [Authorize]
        public JsonResult AdminRights(string username, [FromBody]AdminRightsViewModel viewModel)
        {
            if (viewModel == default(AdminRightsViewModel)) return 204.ErrorStatusCode();
            return UpdateAdminRights(username, viewModel.SetAdmin);
        }

        /// <summary>
        /// Delete user with the specified <paramref name="username"/>.
        /// </summary>
        /// <param name="username">Unique identifier of the user.</param>
        /// <returns>HTTP status code indicating outcome of the delete operation.</returns>
        [SwaggerResponse(HttpStatusCode.Created, "User profile successfully deleted.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Other errors.")]
        [HttpDelete("{username}")]
        [Authorize]
        public async Task<JsonResult> Delete(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == username.ToUpper());
            if (user == null) return 401.ErrorStatusCode();

            var isAuthorized = await CheckUserCredentials(user.UserName);

            if (isAuthorized.StatusCode != 200 && isAuthorized.StatusCode.HasValue)
                return isAuthorized.StatusCode.Value.ErrorStatusCode();

            if (isAuthorized.StatusCode != 200)
                if (isAuthorized.StatusCode != null) return isAuthorized.StatusCode.Value.ErrorStatusCode();
                else return 400.ErrorStatusCode();

            await _userManager.DeleteAsync(user);
            return 200.SuccessStatusCode();
        }

        private JsonResult UpdateAdminRights(string username, bool? setAdmin)
        {
            // check that user is logged in
            var currUser = GetCurrentUser();
            if (currUser == default(User)) return 401.ErrorStatusCode();

            // check that the current user is admin
            if (!currUser.IsAdmin) return 403.ErrorStatusCode();

            // check that user in model exists
            _context.Users.Load();
            var user = _context.Users.FirstOrDefault(u => u.Id == username);
            if (user == default(User))
                return
                    404.ErrorStatusCode(new Dictionary<string, string>()
                    {
                        {"user", "User with the specified username not found"}
                    });

            if (setAdmin == null) return 304.SuccessStatusCode();

            // update
            user.IsAdmin = setAdmin.Value;
            _context.SaveChanges();
            return user.ToJson();
        }

        private async Task<JsonResult> CheckUserCredentials(string id)
        {
            // User null, how did we even get past the Authorize attribute?
            var user = await _userManager.GetUser(User.Identity.Name);
            if (user == null) return 401.ErrorStatusCode();

            // This user does not have enough rights
            if (user.UserName != id) return 403.ErrorStatusCode();
            return 200.SuccessStatusCode();
        }

        private User GetCurrentUser()
        {
            _context.Users.AsNoTracking().Load();
            var user = _userManager.GetUserId(User);
            if (user == null) return null;
            var userObj = _context.Users.FirstOrDefault(u => u.Id == user);
            if (userObj == default(User)) return null;
            return userObj;
        }
    }
}