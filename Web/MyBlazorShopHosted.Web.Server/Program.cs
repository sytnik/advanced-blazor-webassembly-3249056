using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using MyBlazorShopHosted.Libraries.Services.Product;
using MyBlazorShopHosted.Libraries.Services.ShoppingCart;
using MyBlazorShopHosted.Libraries.Services.Storage;
using System.Net.Http.Headers;
using MyBlazorShopHosted.Web.Client.StateManagement;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Dependency injection
builder.Services.AddSingleton<IStorageService, StorageService>();
builder.Services.AddSingleton<IShoppingCartService, ShoppingCartService>();
builder.Services.AddTransient<IProductService, ProductService>();

builder.Services.AddSingleton(serviceProvider => {
    var addressFeature = serviceProvider.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
    var baseAddress = addressFeature?.Addresses.First();

    var http = new HttpClient()
    {
        BaseAddress = new Uri(baseAddress)
    };
    http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
    {
        NoCache = true
    };
    return http;
});
builder.Services.AddScoped<IShoppingCartStateContainer, ShoppingCartStateContainer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    // No cache
    app.Use(async (httpContext, next) =>
    {
        httpContext.Response.Headers[HeaderNames.CacheControl] = "no-cache";
        await next();
    });
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("/ProductListing");

app.Run();