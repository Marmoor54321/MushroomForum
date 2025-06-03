// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;


namespace MushroomForum.Areas.Identity.Pages.Account.Manage
{
    public class IconItem
    {
        public string Value { get; set; } // np. "icon1.png"
        public string Url { get; set; }   // np. "/icons/icon1.png"
    }

    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public IndexModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }


        public List<IconItem> AvailableIcons { get; set; } = new();
        public string SelectedIcon { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public string Username { get; set; }


        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Wybrana ikona")]
            public string SelectedIcon { get; set; }


        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                SelectedIcon = profile?.AvatarIcon
            };

            SelectedIcon = profile?.AvatarIcon;
        }


        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new UserProfile { UserId = user.Id };
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            AvailableIcons = new List<IconItem>
            {
                new IconItem { Value = "icon1.png", Url = "/icons/icon1.png" },
                new IconItem { Value = "icon2.png", Url = "/icons/icon2.png" },
                new IconItem { Value = "icon3.png", Url = "/icons/icon3.png" }
            };

            await LoadAsync(user);

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
    }

    if (!ModelState.IsValid)
    {
        await LoadAsync(user);
        return Page();
    }

    // Zmień nazwę użytkownika, jeśli została zmieniona
    var currentUserName = await _userManager.GetUserNameAsync(user);
    if (Username != currentUserName)
    {
        var setUserNameResult = await _userManager.SetUserNameAsync(user, Username);
        if (!setUserNameResult.Succeeded)
        {
            StatusMessage = "Błąd przy zmianie nazwy użytkownika.";
            return RedirectToPage();
        }
    }

    // Zaktualizuj avatar w UserProfile
    var existingProfile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
    if (existingProfile != null && !string.IsNullOrEmpty(Input.SelectedIcon))
    {
        existingProfile.AvatarIcon = Input.SelectedIcon;
        await _context.SaveChangesAsync();
    }

    await _signInManager.RefreshSignInAsync(user);
    StatusMessage = "Twój profil został zaktualizowany.";
    return RedirectToPage();
}



    }
}

