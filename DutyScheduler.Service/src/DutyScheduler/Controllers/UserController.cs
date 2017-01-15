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
            if (user == null) return 404.ErrorStatusCode(Constants.UserNotFound.ToDict());
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
            var users = _context.Users.ToList().OrderBy(u=>u.LastName).ThenBy(u=>u.Name).ThenBy(u=>u.Email);
            return users.ToJson();
        }

        /// <summary>
        /// Admin can register a new user.
        /// </summary>
        /// <param name="viewModel">User profile to be created.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Created, "User profile successfully created.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.Forbidden, "User does not have the sufficient rights to perform the action.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Validation errors.")]
        [HttpPost]
        [Authorize]
        //[AllowAnonymous]
        public async Task<JsonResult> Create([FromBody]RegisterViewModel viewModel)
        {
            // check that user is logged in
            var currUser = GetCurrentUser();
            if (currUser == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            // check that the current user is admin
            if (!currUser.IsAdmin) return 403.ErrorStatusCode(Constants.Forbidden.ToDict());

            _context.Users.Load();
            var usernames = _context.Users.Select(u => u.NormalizedUserName);
            if (usernames.Contains(viewModel.UserName.ToUpper()))
                ModelState.AddModelError("UserName", "The specified username is taken.");

            var emails = _context.Users.Select(u => u.NormalizedEmail);
            if (emails.Contains(viewModel.Email.ToUpper()))
                ModelState.AddModelError("Email", "There already exists an account registered with the specified email.");

            //if (viewModel.Password.Length < 4)
            //    ModelState.AddModelError("Password", "Password must contain at least 4 characters.");

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
        /// Change password.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, "Password successfully changed.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Validation errors.")]
        [HttpPut("password")]
        [Authorize]
        public ActionResult ChangePassword([FromBody] PasswordViewModel model)
        {
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized);
            if (ModelState.IsValid)
            {
                var h = new PasswordHasher<User>();
                user.PasswordHash = h.HashPassword(user, model.Password);
                _context.Users.Update(user);
                _context.SaveChanges();

                return 200.SuccessStatusCode(Constants.PasswordChangeSuccess.ToDict());
            }
            return 400.ErrorStatusCode(ModelState.ValidationErrors());
        }

        /// <summary>
        /// Update user details.
        /// </summary>
        /// <param name="viewModel">New user data.</param>
        /// <returns>JSON user data.</returns>
        [SwaggerResponse(HttpStatusCode.OK, "User details successfully saved.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [HttpPut]
        [Authorize]
        public JsonResult Update([FromBody]UpdateUserViewModel viewModel)
        {
            var user = GetCurrentUser();
            if (user == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized);

            if (viewModel.Name != null) user.Name = viewModel.Name;
            if (viewModel.LastName != null) user.LastName = viewModel.LastName;
            if (viewModel.Office != null) user.Office = viewModel.Office;
            if (viewModel.Phone != null) user.Phone = viewModel.Phone;

            _context.Users.Update(user);
            _context.SaveChanges();

            return user.ToJson();
        }

        /// <summary>
        /// Admin can grant or revoke admin rigts from user with the specified <paramref name="username"/>.
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
            if (viewModel == default(AdminRightsViewModel)) return 204.ErrorStatusCode(Constants.NoContent.ToDict());
            return UpdateAdminRights(username, viewModel.SetAdmin);
        }

        /// <summary>
        /// Admin can delete user with the specified <paramref name="username"/>.
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
            var user = GetCurrentUser();
            if (user == null) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            if (!user.IsAdmin) return 403.ErrorStatusCode(Constants.Forbidden.ToDict());

            var userToDelete = await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
            if (userToDelete == null) return 404.ErrorStatusCode(Constants.UserNotFound.ToDict());

            await _userManager.DeleteAsync(userToDelete);
            return 200.SuccessStatusCode(Constants.OK.ToDict());
        }

        private JsonResult UpdateAdminRights(string username, bool? setAdmin)
        {
            // check that user is logged in
            var currUser = GetCurrentUser();
            if (currUser == default(User)) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

            // check that the current user is admin
            if (!currUser.IsAdmin) return 403.ErrorStatusCode(Constants.Forbidden.ToDict());

            // check that user in model exists
            _context.Users.Load();
            var user = _context.Users.FirstOrDefault(u => u.Id == username);
            if (user == default(User))
                return 404.ErrorStatusCode(Constants.UserNotFound.ToDict());

            if (setAdmin == null) return 304.SuccessStatusCode(Constants.NotModified.ToDict());

            // update
            user.IsAdmin = setAdmin.Value;
            _context.SaveChanges();
            return user.ToJson();
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