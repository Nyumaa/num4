using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using num4.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using num4.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using num4.Data;
using System.Web;
using System.Transactions;

namespace num4.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        
        private AppDbContext _ctx;

        public HomeController(UserManager<User> userManager, AppDbContext ctx)
        {
            _userManager = userManager;
            _ctx = ctx;
           
        }

        [Authorize(Roles = "User")]
        public IActionResult Index() => View(_userManager.Users);

        [HttpGet]
        public async Task<IActionResult> Delete(string users)
        {
            bool result = true;
            if (!string.IsNullOrEmpty(users)) 
            {
                var userList = users.Split(" ");
                result = await DeleteUsers(userList);         
            }
            return result ?  Json(new { redirectToUrl = Url.Action("Index") }) : Json(new { redirectToUrl = Url.Action("Logout", "Account") }); 
        }

        [HttpGet]
        public async Task<IActionResult> Block(string users)
        {
            bool result = true;
            if (!string.IsNullOrEmpty(users))
            {
                var userList = users.Split(" ");
                result = await ChangeStatusUser(userList, true);
            }
            return result ? Json(new { redirectToUrl = Url.Action("Index") }) : Json(new { redirectToUrl = Url.Action("Logout", "Account") });
        }

        [HttpGet]
        public async Task<IActionResult> Unblock(string users)
        {
            bool result = true;
            if (!string.IsNullOrEmpty(users))
            {
                var userList = users.Split(" ");
                result = await ChangeStatusUser(userList, false);
            }
            return result ? Json(new { redirectToUrl = Url.Action("Index") }) : Json(new { redirectToUrl = Url.Action("Logout", "Account") });
        }

        private async Task<bool> ChangeStatusUser(string[] users, bool state)
        {
            bool result = true;
                    foreach (var user in users)
                    {
                        User profile = _userManager.FindByIdAsync(user).Result;
                        profile.Status = state;
                        await _userManager.UpdateAsync(profile);
                            if (state)
                            {
                                await _userManager.UpdateSecurityStampAsync(profile);
                            }
                
                        if (profile.UserName == User.Identity.Name && state)
                        {
                            result = false;
                        }
                    }
            return result;
        }
        private async Task<bool> DeleteUsers(string[] users)
        {
            bool result = true;
            using (var context = _ctx)
            {
                using(var test = context.Database.BeginTransaction())
                {
                    foreach(var user in users)
                    {
                        
                        User profile = _userManager.FindByIdAsync(user).Result;
                       
                        var rolesForUser = await _userManager.GetRolesAsync(profile);
                        if (rolesForUser.Count() > 0)
                        {
                            foreach (var item in rolesForUser.ToList())
                            {
                                await _userManager.RemoveFromRoleAsync(profile, item);
                            }
                        }
                        
                        await _userManager.DeleteAsync(profile);
                        test.Commit();
                        if (profile.UserName == User.Identity.Name)
                        {
                            result = false;
                        }     
                    } 
                }
            }
            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
