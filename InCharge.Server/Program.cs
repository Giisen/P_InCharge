using InCharge.DataAccess.Contexts;
using InCharge.DataAccess.Respositories;
using InCharge.Server.Endpoints;
using InCharge.Server.Services;
using InCharge.Shared.DTOs;
using InCharge.Shared.Interfaces;
using InCharge.Shared.Interfaces.Charge;
using InCharge.Shared.Interfaces.Cost;
using InCharge.Shared.Interfaces.Rate;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<MongoDb>();
builder.Services.AddScoped<MongoDbEvChargeRepository>();
builder.Services.AddScoped<MongoDbHourlyRateRepository>();
builder.Services.AddScoped<MongoDbDailyRateRepository>();
builder.Services.AddScoped<MongoDbDailyCostRepository>();
builder.Services.AddScoped<MongoDbMonthlyCostRepository>();
builder.Services.AddScoped<MongoDbYearlyCostRepository>();
builder.Services.AddScoped<EvChargeServices>();
builder.Services.AddScoped<IEvChargeHourly<EvChargeHourlyDto>, EvChargeServices>();
builder.Services.AddScoped<IEvChargeDaily<EvChargeDailyDto>, EvChargeServices>();
builder.Services.AddScoped<IEvChargeDaily<bool>, EvChargeServices>();
builder.Services.AddScoped<IEvChargeDaily<string>, EvChargeServices>();


builder.Services.AddScoped<RateServices>();
builder.Services.AddScoped<IRate<HourlyRateDto>, RateServices>();
builder.Services.AddScoped<IRate<DailyRateDto>, RateServices>();
builder.Services.AddScoped<IRate<List<DailyRateDto>>, RateServices>();
builder.Services.AddScoped<IRate<bool>, RateServices>();
builder.Services.AddScoped<IRate<CurrentRateDto>, RateServices>();

builder.Services.AddScoped<EvCostServices>();
builder.Services.AddScoped<IEvCostHourly<EvCostHourlyDto>, EvCostServices>();
builder.Services.AddScoped<IEvCostDaily<EvCostDailyDto>, EvCostServices>();
builder.Services.AddScoped<IEvCostMonthly<EvCostMonthlyDto>, EvCostServices>();
builder.Services.AddScoped<IEvCostYearly<EvCostYearlyDto>, EvCostServices>();

// builder.Services.AddHttpClient();
// builder.Services.AddHostedService<FetchDataBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

//app.UseAuthorization();
app.UseRouting();

app.MapEvChargeEndpoints();
app.MapRateEndpointExtensions();
app.MapEvCostEndpoints();
app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
