using FinanceDashboard.Client.Pages;
using FinanceDashboard.Client.Services;
using FinanceDashboard.Components;
using FinanceDashboard.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IExpenseService, ExpenseService>();

builder.Services.AddDbContext<FinanceDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("FinanceDbContext"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

    if (app.Environment.IsDevelopment())
    {
        // Create the database + tables if they don't exist yet.
        // Once you add EF migrations (dotnet ef migrations add InitialCreate)
        // this switches to Migrate() automatically.
        if (dbContext.Database.GetMigrations().Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        // One-time import of wwwroot/expenses.csv. Does nothing once the table has rows.
        var csvPath = Path.Combine(app.Environment.WebRootPath, "expenses.csv");
        var imported = await FinanceDbSeeder.SeedFromCsvAsync(dbContext, csvPath, app.Logger);
        var total = await dbContext.Expenses.CountAsync();

        if (imported == 0 && total > 0)
        {
            app.Logger.LogInformation(
                "Expenses table already contains {Total} rows, CSV import skipped.", total);
        }
        else
        {
            app.Logger.LogInformation(
                "Database ready: {Imported} expenses imported from CSV, {Total} in total.", imported, total);
        }
    }
    else if (!await dbContext.Database.CanConnectAsync())
    {
        throw new InvalidOperationException("Cannot connect to the FinanceDbContext database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(FinanceDashboard.Client._Imports).Assembly);

app.Run();
