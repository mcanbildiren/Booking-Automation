# 🚀 Running the Booking System

This guide contains the commands to run the WhatsApp Booking Service, Admin Panel, and manage the database.

## 📋 Prerequisites

Ensure you have Docker and Docker Compose installed.

## 1️⃣ Start the WhatsApp Service & Database

This starts the core booking service, PostgreSQL database, and ngrok tunnel.

```bash
cd MetaEntegration/WhatsAppBookingService

# Start services (WhatsApp + DB + ngrok)
docker-compose -f docker-compose.override.yml up -d
```

## 2️⃣ Start the Admin Panel

This starts the web-based admin interface.

```bash
cd AdminPanel/HairdresserAdmin

# Start Admin Panel (connects to the existing database)
docker-compose up -d
```

## 🌐 Get Your Webhook URL (ngrok)

After starting the WhatsApp service, run this to get your public webhook URL for Meta:

```bash
curl -s http://localhost:4040/api/tunnels | grep -o '"public_url":"https://[^"]*' | grep -o 'https://[^"]*' | head -1
```

Then append `/api/webhook` to the URL.

**Example:**
`https://lloyd-overattentive-shala.ngrok-free.dev/api/webhook`

## 🛑 Stop All Services

To stop everything cleanly:

```bash
docker stop $(docker ps -q)
```

## 📊 Access Points

- **Admin Panel:** [http://localhost:5002](http://localhost:5002)
  - Login: `admin` / `admin123`
- **WhatsApp Service:** [http://localhost:5001](http://localhost:5001)
- **ngrok Dashboard:** [http://localhost:4040](http://localhost:4040)
- **Database:** `localhost:5432`

## 🛠️ Troubleshooting

**View Logs:**

```bash
# WhatsApp Service
docker logs -f whatsappbookingservice-whatsapp-service-1

# Admin Panel
docker logs -f hairdresseradmin-admin-panel-1
```

**Restart Specific Service:**

```bash
docker restart whatsappbookingservice-whatsapp-service-1
```

