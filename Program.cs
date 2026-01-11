using Chatbot.Api.Models;
using Chatbot.Api.Services;

var builder = WebApplication.CreateBuilder(args);
const string ADMIN_PASSWORD = "PP123!";

// 🔑 ORDINE CORECTĂ: config sources
builder.Configuration
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

// 🔧 Bind Chatbot config
builder.Services.Configure<ChatbotConfig>(
    builder.Configuration.GetSection("Chatbot")
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

// 🧠 Services
builder.Services.AddSingleton<ChatService>();
builder.Services.AddSingleton<SupabaseService>();

var app = builder.Build();

// 🌐 Middleware
app.UseCors(x =>
    x.AllowAnyOrigin()
     .AllowAnyMethod()
     .AllowAnyHeader());

app.UseSwagger();
app.UseSwaggerUI();

// =======================
// 🔵 API ENDPOINTS
// =======================

// 🔹 Chat endpoint
app.MapPost("/chat", async (
    ChatRequest request,
    ChatService chatService) =>
{
    var answer = await chatService.Ask(request.SiteId, request.Question);
    return Results.Ok(new { answer });
});

// 🔹 Lead endpoint (FORMULAR)
app.MapPost("/lead", async (
    LeadRequest request,
    SupabaseService supabase) =>
{
    await supabase.CreateLead(
        request.Name,
        request.Email,
        request.Site
    );

    return Results.Ok(new { success = true });
});

app.MapGet("/leads", async (
    HttpContext context,
    SupabaseService supabase) =>
{
    if (!context.Request.Headers.TryGetValue("X-Admin-Password", out var password) ||
        password != ADMIN_PASSWORD)
    {
        return Results.Unauthorized();
    }

    var leads = await supabase.GetLeads();
    return Results.Ok(leads);
});


// 🌐 Railway PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();

// =======================
// 🔵 REQUEST MODELS
// =======================
record ChatRequest(string SiteId, string Question);
record LeadRequest(string Name, string Email, string Site);
