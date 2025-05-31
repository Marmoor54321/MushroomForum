using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;
using QuestPDF.Infrastructure;
using System.ComponentModel;
using System.Globalization;
//generowanie PDF
var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
//poprawne działanie koordynatów mapy
var cultureInfo = new System.Globalization.CultureInfo("en-US");
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; //100 mb
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<MushroomForum.Services.MushroomIdService>();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Information); 
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    await dbContext.Database.MigrateAsync();
    await SeedDataAsync(dbContext, userManager, roleManager);
}
//wywołanie quizu
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ApplicationDbContext.Seed(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

async Task SeedDataAsync(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    if (!dbContext.Categories.Any())
    {
        dbContext.Categories.AddRange(
            new Category { Name = "Jadalne" },
            new Category { Name = "Trujące" }
        );
        await dbContext.SaveChangesAsync();
    }

    const string userName = "abcd@gmail.com";
    const string password = "Qwerty1$";
    if (await userManager.FindByNameAsync(userName) == null)
    {
        var user = new IdentityUser
        {
            UserName = userName,
            Email = userName,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception("Nie udało się utworzyć użytkownika testowego: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    var testUser = await userManager.FindByNameAsync(userName);

    if (!dbContext.ForumThreads.Any())
    {
        var edibleCategory = await dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Jadalne");
        if (edibleCategory == null)
        {
            throw new Exception("Kategoria 'Jadalne' nie została znaleziona.");
        }

        var thread = new ForumThread
        {
            Title = "Przykładowy wątek o grzybach jadalnych",
            Description = "To jest przykładowy wątek dotyczący grzybów jadalnych",
            IdentityUserId = testUser.Id,
            CategoryId = edibleCategory.CategoryId
        };
        dbContext.ForumThreads.Add(thread);
        await dbContext.SaveChangesAsync();
    }

    if (!dbContext.Posts.Any())
    {
        var thread = await dbContext.ForumThreads.FirstOrDefaultAsync();
        if (thread == null)
        {
            throw new Exception("Wątek nie został znaleziony.");
        }

        var post = new Post
        {
            Description = "To jest przykładowy post w wątku o grzybach jadalnych.",
            IdentityUserId = testUser.Id,
            ForumThreadId = thread.ForumThreadId
        };
        dbContext.Posts.Add(post);
        await dbContext.SaveChangesAsync();
    }
}