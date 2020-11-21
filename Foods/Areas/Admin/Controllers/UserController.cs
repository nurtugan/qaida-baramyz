using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Foods.Data;
using Foods.Utility;

namespace Foods.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);  

            return View(await _db.ApplicationUsers.Where(x => x.Id != claim.Value).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);  

            var userList = await _db.ApplicationUsers.Where(x => x.Id != claim.Value).ToListAsync();
            var userRole = await _db.UserRoles.ToListAsync();
            var roles = await _db.Roles.ToListAsync();
            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (applicationUser == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }
            if (applicationUser.LockoutEnd != null && applicationUser.LockoutEnd > DateTime.Now)
            {
                applicationUser.LockoutEnd = DateTime.Now;
            }
            else
            {
                applicationUser.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful." });
        }
    }
}