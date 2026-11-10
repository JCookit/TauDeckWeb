var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TauWeb.Web.Services.TauEditionCatalog>();
builder.Services.AddScoped<TauWeb.Web.Services.TauEditionContext>();
builder.Services.AddScoped<TauWeb.Web.Services.TauContentStore>();
builder.Services.AddSingleton<TauWeb.Web.Services.TauArticleStore>();
builder.Services.Configure<TauWeb.Web.Services.TauGameStoreOptions>(builder.Configuration.GetSection("GameStore"));
if (string.Equals(builder.Configuration["GameStore:Provider"], "Cosmos", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<TauWeb.Web.Services.ITauGameRepository, TauWeb.Web.Services.CosmosTauGameRepository>();
    builder.Services.AddHostedService<TauWeb.Web.Services.TauGameSeedHostedService>();
}
else
{
    builder.Services.AddSingleton<TauWeb.Web.Services.ITauGameRepository, TauWeb.Web.Services.JsonTauGameRepository>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.Use(async (context, next) =>
{
    var editions = context.RequestServices.GetRequiredService<TauWeb.Web.Services.TauEditionCatalog>();
    if (!context.Request.Path.StartsWithSegments("/games") && context.Request.Query.ContainsKey("edition")
        && (context.Request.Query["edition"].Count != 1 || !editions.Editions.Any(e => e.Id == context.Request.Query["edition"].ToString())))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsync("Unknown deck edition.");
        return;
    }
    await next();
});

app.UseAuthorization();

app.MapStaticAssets();
app.MapGet("/generated/{**path}", (string path, TauWeb.Web.Services.TauEditionContext edition) =>
    Results.Redirect(edition.Asset("/generated/" + path)));
app.MapGet("/content/tau-data.json", (TauWeb.Web.Services.TauEditionContext edition) =>
    Results.Redirect($"/api/tau-data?edition={edition.Id}"));
app.MapGet("/content/system-text.json", (TauWeb.Web.Services.TauEditionContext edition) =>
    Results.Redirect($"/editions/{edition.Id}/content/system-text.json"));
app.MapGet("/api/tau-data", async (TauWeb.Web.Services.TauContentStore store) => await store.GetAsync());
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

public partial class Program { }
