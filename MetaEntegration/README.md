# WhatsApp Booking Service - Meta Integration

A C# ASP.NET Core service that integrates with Meta's WhatsApp Business Cloud API to provide automated appointment booking via WhatsApp.

## Features

- ✅ **WhatsApp Cloud API Integration** - Direct integration with Meta's official API
- ✅ **Interactive Messages** - Uses buttons and lists for better UX
- ✅ **Conversation Flow** - Stateful conversation management for booking appointments
- ✅ **PostgreSQL Integration** - Connects to your existing database
- ✅ **Real-time Availability** - Shows only available time slots
- ✅ **Turkish Language Support** - All messages in Turkish
- ✅ **Appointment Management** - Create and cancel appointments
- ✅ **Webhook Handler** - Receives messages from WhatsApp in real-time

## Architecture

```
┌─────────────────┐
│   WhatsApp      │
│   Business API  │
└────────┬────────┘
         │ Webhook
         ▼
┌─────────────────────┐
│  WebhookController  │
└─────────┬───────────┘
          │
          ▼
┌─────────────────────┐       ┌──────────────────┐
│  MessageHandler     │◄──────│ ConversationSvc  │
└─────────┬───────────┘       └──────────────────┘
          │
          ▼
┌─────────────────────┐       ┌──────────────────┐
│  BookingService     │◄──────│ WhatsAppService  │
└─────────┬───────────┘       └──────────────────┘
          │
          ▼
┌─────────────────────┐
│    PostgreSQL DB    │
└─────────────────────┘
```

## Prerequisites

1. **Meta Developer Account** - [developers.facebook.com](https://developers.facebook.com)
2. **WhatsApp Business Account** - Set up through Meta Business Suite
3. **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download)
4. **PostgreSQL** - Your existing database

## Setup Guide

### 1. Meta WhatsApp Business API Setup

1. Go to [Meta for Developers](https://developers.facebook.com)
2. Create a new app or select existing one
3. Add "WhatsApp" product to your app
4. Navigate to WhatsApp > API Setup
5. Note down:
   - **Phone Number ID** (from "From" dropdown)
   - **Access Token** (generate a permanent token)
   - **Webhook Verify Token** (you create this - any string)

### 2. Configure Webhook on Meta

1. In Meta Developer Console, go to WhatsApp > Configuration
2. Click "Edit" next to Webhook
3. Enter your webhook URL: `https://your-domain.com/api/webhook`
4. Enter your Verify Token (same as in appsettings.json)
5. Subscribe to these webhook fields:
   - `messages`

### 3. Configure the Application

1. Copy `appsettings.example.json` to `appsettings.json`:
```bash
cd /Users/mcanbildiren/Documents/Repos/BookingSystem/MetaEntegration/WhatsAppBookingService
cp appsettings.example.json appsettings.json
```

2. Edit `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hairdresser_booking;Username=your_username;Password=your_password"
  },
  "WhatsApp": {
    "PhoneNumberId": "123456789012345",
    "AccessToken": "EAAxxxxxxxxxxxxxxxxxxxxx",
    "VerifyToken": "my_super_secret_verify_token_12345"
  }
}
```

### 4. Run the Application

```bash
# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

The service will start on `https://localhost:5001` (or the port specified in launchSettings.json)

### 5. Expose Webhook (Development)

For development, you need to expose your local server to the internet. Use one of these tools:

**Using ngrok:**
```bash
ngrok http 5001
```

Then use the ngrok URL (e.g., `https://abc123.ngrok.io/api/webhook`) in Meta's webhook configuration.

## Usage

Once set up, users can interact with your WhatsApp Business number:

### Commands

- `/randevu` - Start booking a new appointment
- `/iptal` - Cancel an existing appointment
- `/yardim` - Show help message

### Booking Flow

1. User sends `/randevu`
2. System shows available dates (next 7 days)
3. User selects a date
4. System shows available time slots
5. User selects a time
6. System asks for confirmation
7. User confirms → Appointment created ✅

### Cancellation Flow

1. User sends `/iptal`
2. System shows user's active appointments
3. User selects appointment to cancel
4. Appointment cancelled ✅

## Project Structure

```
WhatsAppBookingService/
├── Controllers/
│   └── WebhookController.cs       # Webhook endpoint
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext
├── Models/
│   ├── Appointment.cs             # Appointment entity
│   ├── User.cs                    # User entity
│   ├── BusinessConfig.cs          # Configuration entity
│   ├── ConversationState.cs       # Conversation state model
│   └── WhatsAppWebhook.cs         # Webhook payload models
├── Services/
│   ├── IWhatsAppService.cs        # WhatsApp API interface
│   ├── WhatsAppService.cs         # WhatsApp API implementation
│   ├── IBookingService.cs         # Booking logic interface
│   ├── BookingService.cs          # Booking logic implementation
│   ├── IConversationService.cs    # State management interface
│   ├── ConversationService.cs     # In-memory state management
│   ├── IMessageHandler.cs         # Message processing interface
│   └── MessageHandler.cs          # Message processing logic
├── appsettings.json               # Configuration (gitignored)
├── appsettings.example.json       # Configuration template
└── Program.cs                     # Application entry point
```

## Features Explained

### 1. WhatsAppService
Handles all communication with Meta's WhatsApp Cloud API:
- Send text messages
- Send interactive buttons (up to 3 buttons)
- Send interactive lists (up to 10 items)
- Mark messages as read

### 2. BookingService
Manages appointment and user data:
- Create/retrieve users
- Get available time slots
- Create appointments
- Cancel appointments
- Check for conflicts

### 3. ConversationService
Manages conversation state in-memory:
- Track where user is in the booking flow
- Store temporary selections (date, time)
- Clear state after completion

**Note:** For production, consider using Redis for distributed state management.

### 4. MessageHandler
Processes incoming messages and coordinates the flow:
- Parse commands (`/randevu`, `/iptal`, etc.)
- Guide users through booking flow
- Handle interactive replies
- Validate inputs

## API Endpoints

### GET /api/webhook
Webhook verification endpoint for Meta.

**Query Parameters:**
- `hub.mode` - Should be "subscribe"
- `hub.verify_token` - Your verify token
- `hub.challenge` - Challenge string to return

**Response:** Returns the challenge string if verification succeeds.

### POST /api/webhook
Receives messages and status updates from WhatsApp.

**Request Body:** WhatsApp webhook payload (JSON)

**Response:** Always returns 200 OK

## Configuration Options

### Database Connection
Update in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=hairdresser_booking;Username=user;Password=pass"
}
```

### Business Hours
Configure in PostgreSQL `business_config` table:
```sql
UPDATE business_config SET config_value = '9' WHERE config_key = 'business_start_hour';
UPDATE business_config SET config_value = '18' WHERE config_key = 'business_end_hour';
UPDATE business_config SET config_value = '60' WHERE config_key = 'slot_duration_minutes';
```

## Logging

Logs are written to:
- Console (stdout)
- Files in `logs/` directory (rotated daily)

Log files: `logs/whatsapp-booking-YYYYMMDD.txt`

## Security Notes

1. **Never commit `appsettings.json`** with real credentials
2. Use **permanent access tokens** in production (not temporary ones)
3. Use **HTTPS** for webhook endpoint
4. Validate webhook signatures (optional enhancement)
5. Consider rate limiting on webhook endpoint

## Testing

### Test Webhook Locally

1. Start the application
2. Use curl to test:

```bash
# Test verification
curl "http://localhost:5001/api/webhook?hub.mode=subscribe&hub.verify_token=YOUR_TOKEN&hub.challenge=test123"

# Should return: test123
```

### Test WhatsApp Integration

1. Send a message to your WhatsApp Business number
2. Check logs for webhook reception
3. Verify response is sent back

## Deployment

### Using Docker (Recommended)

Create a `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WhatsAppBookingService.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhatsAppBookingService.dll"]
```

Build and run:
```bash
docker build -t whatsapp-booking-service .
docker run -p 80:80 whatsapp-booking-service
```

### Using Azure/AWS/GCP

Deploy as a standard ASP.NET Core application. Ensure:
- PostgreSQL is accessible
- Environment variables or app settings are configured
- HTTPS is enabled
- Port 80/443 is open

## Troubleshooting

### Webhook not receiving messages
1. Check webhook URL is publicly accessible
2. Verify webhook fields are subscribed in Meta console
3. Check logs for incoming requests
4. Ensure verify token matches

### Messages not sending
1. Check access token is valid
2. Verify phone number ID is correct
3. Check API quota limits
4. Review logs for API errors

### Database connection issues
1. Verify connection string is correct
2. Ensure PostgreSQL is running
3. Check user has necessary permissions
4. Test connection with psql

## Extending the System

### Add Service Type Selection

Update `MessageHandler.cs` to add a step for selecting service type (haircut, coloring, etc.)

### Add Payment Integration

Create a new service to handle payments via Stripe/PayPal after appointment confirmation.

### Add Reminder System

Create a background service to send reminders 24 hours before appointments.

### Add Admin Notifications

Send notifications to admin panel when new appointments are created.

## Support

For issues with:
- **Meta WhatsApp API**: Check [Meta Developer Docs](https://developers.facebook.com/docs/whatsapp)
- **This application**: Create an issue in the repository

## License

Private use

---

Built with ❤️ using ASP.NET Core and Meta WhatsApp Business API

