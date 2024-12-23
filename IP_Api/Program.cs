using IP_DAL.Repository;
using Microsoft.EntityFrameworkCore;
using IP_DAL;
using IP_Domain.Interfaces;
using IP_Domain.Services;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IIPAdressRepository, IpAdressRepository>();

builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<HttpService>();

builder.Services.AddHangfire(x => x.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<SqlJobsService>();

//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer("YourConnectionString",
//        sqlServerOptions =>
//        {
//            sqlServerOptions.EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(30), 
//                errorNumbersToAdd: null
//            );
//        }));



var app = builder.Build();

app.UseHangfireDashboard();

// Configuração do pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "IP API"));
}

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<SqlJobsService>(
    "update-ip-job",  
    service => service.UpdateIPJob(),
    "* * * * *" //every hour
                //"*/5 * * * *" //every 5min
                //"0 * * * *" //every 1min
);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
