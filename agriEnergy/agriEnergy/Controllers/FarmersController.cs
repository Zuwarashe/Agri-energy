﻿using agriEnergy.Areas.Identity.Data;
using agriEnergy.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace agriEnergy.Controllers
{
    [Authorize(Roles = "Employee")]
    public class FarmersController : Controller
    {
        private readonly FarmerDbContext _context;
        private readonly UserManager<agriEnergyUser> _userManager; // Inject UserManager

        public FarmersController(FarmerDbContext context, UserManager<agriEnergyUser> userManager)
        {
            _context = context;
            _userManager = userManager; // Initialize UserManager
        }
        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Check if the current user is the special user with email "user@employee.com".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task<bool> IsSpecialUser()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Email == "user@employee.com";
        }
        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Display a list of farmers.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            if (!await IsSpecialUser() && !User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Home");
            }

            var farmers = _context.Farmers.OrderByDescending(p => p.Id).ToList();
            return View(farmers);
        }
        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Display the farmer creation form.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            if (!await IsSpecialUser() && !User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Handle the submission of the farmer creation form.
        /// </summary>
        /// <param name="addfarmers"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create(addFarmers addfarmers)
        {
            if (!await IsSpecialUser() && !User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                var user = new agriEnergyUser
                {
                    UserName = addfarmers.email,
                    Email = addfarmers.email,
                    firstName = addfarmers.name
                };

                var result = await _userManager.CreateAsync(user, addfarmers.password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, addfarmers.role);

                    var farmer = new Farmers
                    {
                        name = addfarmers.name,
                        email = addfarmers.email,
                        password = user.PasswordHash, // use the hashed password from Identity
                        role = addfarmers.role
                    };

                    _context.Farmers.Add(farmer);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Farmers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(addfarmers);
        }
        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Display the farmer editing form.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public async Task<IActionResult> Edit(int id)
        {
            if (!await IsSpecialUser() && !User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Home");
            }

            var farmer = _context.Farmers.Find(id);
            if (farmer == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var editModel = new addFarmers
            {
                name = farmer.name,
                email = farmer.email,
                role = farmer.role,
                password = farmer.password
            };

            ViewData["id"] = id;

            return View(editModel);
        }

        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Handle the submission of the farmer editing form.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="addfarmers"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> Edit(int id, addFarmers addfarmers)
        {
            if (!await IsSpecialUser() && !User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Home");
            }

            var farmer = await _context.Farmers.FindAsync(id);
            if (farmer == null)
            {
                return RedirectToAction("Index", "Products");
            }

            farmer.name = addfarmers.name;
            farmer.email = addfarmers.email;
            farmer.role = addfarmers.role;
            farmer.password = HashPassword(addfarmers.password);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Farmers");
        }
        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Handle the deletion of a farmer.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsSpecialUser() && !User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Home");
            }

            var farmer = await _context.Farmers.FindAsync(id);
            if (farmer == null)
            {
                return RedirectToAction("Index", "Products");
            }

            _context.Farmers.Remove(farmer);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Products");
        }

        //--------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Helper method to hash the password.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}
//---------------------------------------- END OF FILE -------------------------------------------------------//
