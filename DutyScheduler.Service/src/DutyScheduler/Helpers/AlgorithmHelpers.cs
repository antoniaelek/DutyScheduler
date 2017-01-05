using System.Threading.Tasks;
using DutyScheduler.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DutyScheduler.Helpers
{
    public static class AlgorithmHelpers
    {
        public static async Task GetUsers(ApplicationDbContext context, UserManager<User> userManager)
        {
            await context.Users.LoadAsync();
        }
    }
}
