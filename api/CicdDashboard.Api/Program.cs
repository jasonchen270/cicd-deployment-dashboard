using System.Text.Json.Serialization;
using CicdDashboard.Api.CiClients;
using CicdDashboard.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICiClient, GitHubActionsClient>();
builder.Services.AddSingleton<ICiClient, JenkinsClient>();
builder.Services.AddSingleton<BuildAggregator>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:4200", "http://localhost:8080")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.Run();

public partial class Program;
