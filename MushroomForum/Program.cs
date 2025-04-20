using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Data;
using MushroomForum.Models;

var builder = WebApplication.CreateBuilder(args);



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Upewnij się, że baza danych jest utworzona
    await dbContext.Database.MigrateAsync();

    // Sprawdź, czy istnieją użytkownicy i wątki
    var user = await userManager.Users.FirstOrDefaultAsync();
    var forumThread = await dbContext.ForumThreads.FirstOrDefaultAsync();

    if (user != null && forumThread != null)
    {
        // Sprawdź, czy przykładowy post już istnieje, aby uniknąć duplikatów
        if (!await dbContext.Posts.AnyAsync(p => p.Title == "Test Post"))
        {
            var newPost = new Post
            {
                Title = "Test Post",
                Description = "This is a test post created programmatically.",
                ForumThreadId = forumThread.ForumThreadId,
                IdentityUserId = user.Id
            };

            dbContext.Posts.Add(newPost);
            await dbContext.SaveChangesAsync();
            Console.WriteLine("Przykładowy post został utworzony.");
        }
    }
    else
    {
        Console.WriteLine("Nie można utworzyć posta: brak użytkownika lub wątku w bazie danych.");
    }
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
