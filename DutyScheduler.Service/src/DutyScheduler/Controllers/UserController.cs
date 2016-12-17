using System.Linq;
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

namespace DutyScheduler.Controllers 
{
    /// <summary>
    /// User resource.
    /// </summary>
    [Route("api/[controller]/")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
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
        /// Get user with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique identifier of the user.</param>
        /// <returns>JSON user data.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<JsonResult> Get(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == id);
            return new JsonResult(new
            {
                Success = true, user.Id, user.UserName, user.Name, user.Email, user.DateCreated
            });
        }

        /// <summary>
        /// Register.
        /// </summary>
        /// <param name="viewModel">User profile to be created.</param>
        /// <returns>New user JSON data.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Create([FromBody]RegisterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = viewModel.UserName,
                    Email = viewModel.Email,
                    Name = viewModel.Name
                };

                var result = await _userManager.CreateAsync(user, viewModel.Password);

                if (result.Succeeded)
                {
                    return new JsonResult(new {
                        Success = true, user.Id, user.UserName, user.Name, user.Email, user.DateCreated
                    });
                }
            
            }
            if (!ModelState.Keys.Any()) 
                ModelState.AddModelError("Email","There already exists an account with that email.");
            //var allErrors = ModelState.ValidationErrors();
            var ret = new JsonResult(new { Success = false/*, Verbose = allErrors*/});
            ret.StatusCode = 400;
            return ret;
        }

        /// <summary>
        /// Update user with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique identifier of the user.</param>
        /// <param name="viewModel">New user data.</param>
        /// <returns>JSON user data.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<JsonResult> Update(string id, UpdateUserViewModel viewModel)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == id);
            if (user == null) return 404.ErrorStatusCode();

            var isAuthorized = await CheckUserAuthorized(user.UserName);
            if (isAuthorized.StatusCode != 200) return 401.ErrorStatusCode();

            user.Name = viewModel.Name;
            _context.Users.Update(user);
            _context.SaveChanges();

            return new JsonResult(new
            {
                Success = true, user.Id, user.UserName, user.Name, user.Email, user.DateCreated
            });
        }

        /// <summary>
        /// Delete user with the specified <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Unique identifier of the user.</param>
        /// <returns>HTTP status code indicating outcome of the delete operation.</returns>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<JsonResult> Delete(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == id);
            if (user == null) return 404.ErrorStatusCode();

            var isAuthorized = await CheckUserAuthorized(user.UserName);
            if (isAuthorized.StatusCode != 200) return 401.ErrorStatusCode();

            await _userManager.DeleteAsync(user);
            return 200.SuccessStatusCode();
        }

        private async Task<JsonResult> CheckUserAuthorized(string id)
        {
            
            // User null, how did we even get past the Authorize attribute?
            var user = await _userManager.GetUser(User.Identity.Name);
            if (user == null) return 401.ErrorStatusCode();

            // This user is not the one with the specified id
            if (user.UserName != id) return 401.ErrorStatusCode();
            return 200.SuccessStatusCode();
        }
    }
}