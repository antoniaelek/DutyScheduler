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

        private async Task<JsonResult> CheckUserCredentials(string id)
        {
            // User null, how did we even get past the Authorize attribute?
            var user = await _userManager.GetUser(User.Identity.Name);
            if (user == null) return 401.ErrorStatusCode();

            // This user does not have enough rights
            if (user.UserName != id) return 403.ErrorStatusCode();
            return 200.SuccessStatusCode();
        }
    }
}