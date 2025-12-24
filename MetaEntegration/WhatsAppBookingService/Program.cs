using Microsoft.EntityFrameworkCore;
using Serilog;
using WhatsAppBookingService.Data;
using WhatsAppBookingService.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/whatsapp-booking-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageHandler, MessageHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Directory.CreateDirectory("logs");

app.Logger.LogInformation("WhatsApp Booking Service starting...");

app.Run();

