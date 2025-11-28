using Microsoft.EntityFrameworkCore;
using Serilog;
using WhatsAppBookingService.Data;
using WhatsAppBookingService.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/whatsapp-booking-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP Client for WhatsApp API
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();

// Application services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Create logs directory if it doesn't exist
Directory.CreateDirectory("logs");

app.Logger.LogInformation("WhatsApp Booking Service starting...");

app.Run();

