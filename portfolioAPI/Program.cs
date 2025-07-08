using Microsoft.EntityFrameworkCore;
using portfolioAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PortfolioDatabase")));


// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend", policy =>
//         policy.WithOrigins(
//             "https://portfolio-frontend-2-fpt4.onrender.com", 
//             "https://akash-kce.github.io",                    
//             "http://localhost:4200",                        
//             "http://localhost:3000"                          
//         )
//         .AllowAnyHeader()
//         .AllowAnyMethod());
// });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); 

app.UseAuthorization();

app.MapControllers();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://*:{port}");
}

app.Run();
