using LinkUpPro.Core.Application.IoC;
using LinkUpPro.Infrastructure.Identity.IoC;
using LinkUpPro.Infrastructure.Persistence.IoC;
using LinkUpPro.Infrastructure.Shared.IoC;
using LinkUpPro.Services;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
}).AddRazorRuntimeCompilation();

builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddPersistenceInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddIdentityInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddSharedInfrastructure(builder.Configuration);

builder.Services.AddHostedService<BattleshipBackgroundService>();

builder.Services.AddMemoryCache();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles(); 
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
