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
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.SwaggerGen.Annotations;

namespace DutyScheduler.Controllers
{
    [Route("api/[controller]/")]
    public class SessionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly Microsoft.Extensions.Logging.ILogger _logger;
        private IConfigurationRoot _configuration;

        public SessionController(ApplicationDbContext context,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILoggerFactory loggerFactory,
            IHostingEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<UserController>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _configuration = builder.Build();
        }


        /// <summary>
        /// Logout.
        /// </summary>
        /// <returns>>HTTP status code indicating outcome of the operation.</returns>
        [SwaggerResponse(HttpStatusCode.Created, "User successfully logged out.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Other errors.")]
        [HttpDelete]
        [Authorize]
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        /// <summary>
        /// Get logged in user
        /// </summary>
        /// <returns>User object.</returns>
        [SwaggerResponse(HttpStatusCode.OK, "User object.")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, "User is not logged in.")]
        [HttpGet]
        [Authorize]
        public JsonResult Get()
        {
            var user = GetCurrentUser();
            return user.ToJson();
        }

        /// <summary>
        /// Login either using the username and password or email and password combination.
        /// </summary>
        /// <param name="viewModel">User to login.</param>
        /// <returns>>HTTP status code and message indicating outcome of the operation.</returns>
        [SwaggerResponse(HttpStatusCode.Created, "User successfully logged in.")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Validation errors.")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Login([FromBody]LoginViewModel viewModel)
        {
            var key = this.Request.Headers.Keys.FirstOrDefault(h => h.ToLower() == "user-agent");
            var browser =  "";
            if (key != null) browser = this.Request.Headers[key];
            
            if (viewModel.UserName == null && viewModel.Email == null)
            {
                ModelState.AddModelError("Email","Email or Username field is requried.");
                ModelState.AddModelError("Username","Email or Username field is requried.");
            }
            if (ModelState.IsValid)
            {
                User user = null;
                if (viewModel.Email != null) 
                {
                    user = await _userManager.FindByEmailAsync(viewModel.Email);
                    if (user == null) return 404.ErrorStatusCode(Constants.UserNotFound.ToDict());
                }
                else if (viewModel.UserName != null || 
                         (user == null && viewModel.UserName != null)) 
                    user = await _userManager.FindByNameAsync(viewModel.UserName);

                if (user == null) return 401.ErrorStatusCode(Constants.Unauthorized.ToDict());

                var result = await _signInManager.
                    PasswordSignInAsync(user?.UserName,
                                        viewModel.Password,
                                        true, false);
                if (result.Succeeded)
                {
                    user = await _userManager.FindByEmailAsync(user?.Email);
                    return user.ToJson();
                }
            }
            if (!ModelState.Keys.Any()) 
            {
                ModelState.AddModelError("Password","Invalid email or password.");
                ModelState.AddModelError("Email","Invalid email or password.");
            }
            var allErrors = ModelState.ValidationErrors();
            var ret = new JsonResult(new { Success = false, Errors = allErrors });
            ret.StatusCode = 400;
            return ret;
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