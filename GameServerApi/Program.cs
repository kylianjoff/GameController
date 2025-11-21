using Microsoft.EntityFrameworkCore.Sqlite;
using GameServerApi.Models;
using Scalar.AspNetCore;

namespace GameServerApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContext<UserContext>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseAuthorization();


        app.MapControllers();
        app.UseCors("AllowAll");

        app.Run();
    }
}
