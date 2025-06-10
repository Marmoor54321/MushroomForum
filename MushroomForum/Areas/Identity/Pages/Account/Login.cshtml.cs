// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;

namespace MushroomForum.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context; 

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "Email or Username")]
            public string Login { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                string userName = Input.Login;

                // Jeśli login wygląda jak email – znajdź użytkownika po emailu
                if (new EmailAddressAttribute().IsValid(Input.Login))
                {
                    var user = await _signInManager.UserManager.FindByEmailAsync(Input.Login);
                    if (user != null)
                    {
                        userName = user.UserName;
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(userName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    var user = await _signInManager.UserManager.FindByNameAsync(userName);

                    try
                    {
                        await CleanupAndGenerateDailyQuestsAsync(user);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during daily quest cleanup/generation.");
                        
                    }

                    await CleanupAndGenerateDailyQuestsAsync(user);
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }

        private async Task CleanupAndGenerateDailyQuestsAsync(IdentityUser user)
        {
            var today = DateTime.UtcNow.Date;
            var dayOfWeek = DateTime.UtcNow.DayOfWeek;

            var allProgress = await _context.DailyQuestProgresses
                .Where(p => p.UserId == user.Id)
                .ToListAsync();

            var outdatedProgress = allProgress.Where(p => p.Date.Date != today).ToList();
            if (outdatedProgress.Any())
            {
                _context.DailyQuestProgresses.RemoveRange(outdatedProgress);
                await _context.SaveChangesAsync();
            }

            var todayProgress = await _context.DailyQuestProgresses
                .Where(p => p.UserId == user.Id && p.Date.Date == today)
                .ToListAsync();

            if (!todayProgress.Any())
            {
                var todayQuestTypes = await _context.DailyQuestTypes
                    .Where(q => q.DayOfWeek == dayOfWeek)
                    .ToListAsync();

                foreach (var questType in todayQuestTypes)
                {
                    var progress = new DailyQuestProgress
                    {
                        UserId = user.Id,
                        QuestType = questType.QuestType,
                        Date = today,
                        Progress = 0,
                        Completed = false
                    };

                    _context.DailyQuestProgresses.Add(progress);
                }

                await _context.SaveChangesAsync();
            }
        }

    }
}
