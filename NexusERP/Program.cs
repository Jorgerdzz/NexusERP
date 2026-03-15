using Microsoft.EntityFrameworkCore;
using NexusERP.Data;
using NexusERP.Helpers;
using NexusERP.Repositories;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<HelperSessionContextAccessor>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

//Repositories
builder.Services.AddTransient<AccountRepository>();
builder.Services.AddTransient<DepartamentsRepository>();
builder.Services.AddTransient<EmpleadosRepository>();
builder.Services.AddTransient<PayrollRepository>();
builder.Services.AddTransient<AccountingRepository>();
builder.Services.AddTransient<ClienteRepository>();
builder.Services.AddTransient<FacturacionRepository>();
builder.Services.AddTransient<ReportsRepository>();
builder.Services.AddTransient<DashboardRepository>();
builder.Services.AddTransient<ProfileRepository>();


string connectionString = builder.Configuration.GetConnectionString("NexusConnection");
builder.Services.AddDbContext<NexusContext>(options => options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
