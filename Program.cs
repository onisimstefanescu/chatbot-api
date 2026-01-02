using Chatbot.Api.Models;
using Chatbot.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ?? ORDINE CORECTÃ: mai întâi sursele de config
builder.Configuration
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// ?? apoi bind-uirea configului
builder.Services.Configure<ChatbotConfig>(
    builder.Configuration.GetSection("Chatbot")
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

// ?? un singur ChatService, clar
builder.Services.AddSingleton<ChatService>();

var app = builder.Build();

app.UseCors(x =>
    x.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/chat", async (
    ChatRequest request,
    ChatService chatService) =>
{
    var answer = await chatService.Ask(request.SiteId, request.Question);
    return Results.Ok(new { answer });
});

// ?? Railway PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();

record ChatRequest(string SiteId, string Question);
