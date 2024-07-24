using Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ISnapshot, Snapshot>();
builder.Services.AddSingleton<IDiceService, DiceService>();
builder.Services.AddTransient<IItemService, ItemService>();
builder.Services.AddTransient<ICharacterService, CharacterService>();
builder.Services.AddTransient<ITownhallService, TownhallService>();
builder.Services.AddTransient<IActionService, ActionService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
