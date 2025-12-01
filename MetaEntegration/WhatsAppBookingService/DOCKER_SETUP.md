# 🐳 Docker Setup Guide with ngrok

Complete guide to run your WhatsApp Booking Service with Docker and test it with ngrok.

## 📋 Prerequisites

1. **Docker & Docker Compose** installed
   ```bash
   # Check installation
   docker --version
   docker-compose --version
   ```

2. **ngrok account** (free)
   - Sign up at: https://dashboard.ngrok.com/signup
   - Get your auth token: https://dashboard.ngrok.com/get-started/your-authtoken

3. **Meta WhatsApp Business API** credentials
   - Phone Number ID
   - Access Token
   - Verify Token (you create this)

## 🚀 Quick Start

### Step 1: Configure Environment Variables

```bash
cd /Users/mcanbildiren/Documents/Repos/BookingSystem/MetaEntegration/WhatsAppBookingService

# Copy the example file
cp env.example .env

# Edit with your credentials
nano .env
```

**Required configuration in `.env`:**

```bash
# Database (you can keep the default)
DB_PASSWORD=postgres123

# WhatsApp API (get from Meta Developer Console)
WHATSAPP_PHONE_NUMBER_ID=123456789012345
WHATSAPP_ACCESS_TOKEN=EAAxxxxxxxxxxxxx
WHATSAPP_VERIFY_TOKEN=my_secret_verify_token

# ngrok (get from ngrok dashboard)
NGROK_AUTHTOKEN=2abcdefghijklmnop_1234567890ABCDEF
```

### Step 2: Start Everything

```bash
# Make sure you're in the right directory
cd /Users/mcanbildiren/Documents/Repos/BookingSystem/MetaEntegration/WhatsAppBookingService

# Run the start script
./start.sh
```

The script will:
- ✅ Validate your configuration
- ✅ Build the Docker containers
- ✅ Start PostgreSQL database
- ✅ Start WhatsApp service
- ✅ Start ngrok tunnel
- ✅ Display your webhook URL

**Output will show:**

```
========================================
✅ Your Webhook URL is:
https://abc123.ngrok.io/api/webhook
========================================
```

### Step 3: Configure Webhook in Meta

1. Go to: https://developers.facebook.com
2. Navigate to: **Your App → WhatsApp → Configuration**
3. Click **"Edit"** next to Webhook
4. Enter:
   - **Callback URL:** `https://abc123.ngrok.io/api/webhook` (from step 2)
   - **Verify Token:** (same as in your `.env` file)
5. Click **"Verify and Save"** ✅
6. Subscribe to webhook field: **messages** ✅

### Step 4: Test!

Send a message to your WhatsApp Business number:

```
/randevu
```

You should receive an interactive menu! 🎉

## 🛠️ Useful Commands

```bash
# View logs in real-time
./logs.sh
# or
docker-compose logs -f

# Stop all services
./stop.sh
# or
docker-compose down

# Restart everything
./start.sh

# View ngrok dashboard (see all requests)
open http://localhost:4040

# Check service health
docker-compose ps

# Access database directly
docker-compose exec db psql -U postgres -d hairdresser_booking
```

## 📊 Service URLs

Once running, you can access:

| Service | URL | Description |
|---------|-----|-------------|
| **WhatsApp API** | http://localhost:5000 | Your C# service |
| **ngrok Dashboard** | http://localhost:4040 | See all webhook requests |
| **PostgreSQL** | localhost:5432 | Database connection |
| **Public Webhook** | https://xxx.ngrok.io/api/webhook | Public URL for Meta |

## 🔍 Debugging

### View Application Logs

```bash
./logs.sh
```

Look for:
- ✅ `WhatsApp Booking Service starting...`
- ✅ `Now listening on: http://[::]:80`
- ✅ Webhook requests when messages arrive

### View ngrok Traffic

Open http://localhost:4040 in your browser to see:
- All incoming webhook requests from Meta
- Request/response details
- Replay requests for testing

### Check Database

```bash
# Connect to database
docker-compose exec db psql -U postgres -d hairdresser_booking

# View users
SELECT * FROM users;

# View appointments
SELECT * FROM appointments;

# Exit
\q
```

### Common Issues

**Problem:** ngrok shows "Invalid Auth Token"
```bash
# Solution: Check your NGROK_AUTHTOKEN in .env file
cat .env | grep NGROK_AUTHTOKEN
```

**Problem:** Webhook verification fails
```bash
# Solution: Ensure WHATSAPP_VERIFY_TOKEN matches in both:
# 1. .env file
# 2. Meta Developer Console webhook settings
```

**Problem:** Database connection error
```bash
# Solution: Wait for database to be ready
docker-compose logs db

# Or restart services
docker-compose restart
```

**Problem:** Can't receive messages
```bash
# Check if services are running
docker-compose ps

# Check logs
docker-compose logs whatsapp-service

# Verify webhook subscription in Meta console
```

## 🔄 Updating Code

If you make changes to the C# code:

```bash
# Rebuild and restart
docker-compose down
docker-compose up -d --build

# Or use the script
./start.sh
```

## 🌐 ngrok URL Changes

⚠️ **Important:** ngrok URLs change every time you restart (on free plan)

When you restart:
1. Run `./start.sh` to get new URL
2. Update webhook URL in Meta Developer Console
3. Click "Verify and Save" again

**Solution for permanent URL:**
- Upgrade to ngrok paid plan for static domain
- Or deploy to production server

## 📦 What's Running?

The Docker setup includes:

1. **whatsapp-service** (Your C# app)
   - Receives webhooks from Meta
   - Processes messages
   - Sends responses via WhatsApp API

2. **db** (PostgreSQL)
   - Stores users and appointments
   - Auto-initialized with schema

3. **ngrok**
   - Creates public tunnel to your local service
   - Provides webhook URL for Meta

## 🚀 Deployment to Production

For production, replace ngrok with:

### Option 1: Cloud Provider
- Azure App Service
- AWS Elastic Beanstalk
- Google Cloud Run
- DigitalOcean App Platform

### Option 2: VPS with Domain
- Get a VPS (DigitalOcean, Linode, AWS EC2)
- Point domain to VPS
- Use nginx + Let's Encrypt for HTTPS
- Run docker-compose on VPS

### Option 3: Kubernetes
- Use the Dockerfile
- Deploy to AKS, EKS, or GKE
- Manage with Helm charts

## 💡 Tips

1. **Keep ngrok dashboard open** (http://localhost:4040) to see webhook traffic

2. **Use logs.sh** to watch real-time logs while testing

3. **Test webhook manually:**
   ```bash
   curl "http://localhost:5000/api/webhook?hub.mode=subscribe&hub.verify_token=YOUR_TOKEN&hub.challenge=test123"
   ```

4. **Check ngrok URL anytime:**
   ```bash
   curl -s http://localhost:4040/api/tunnels | grep public_url
   ```

5. **Database persistence:** Data is stored in Docker volume `postgres_data` and persists between restarts

## 🔐 Security Notes

- ✅ `.env` is in `.gitignore` - never commit it
- ✅ Use strong verify token
- ✅ Keep access token secret
- ✅ Change default database password

## 📞 Support

If you encounter issues:

1. Check logs: `./logs.sh`
2. View ngrok traffic: http://localhost:4040
3. Verify Meta webhook configuration
4. Check service status: `docker-compose ps`

---

**Happy Testing! 🎉**

Your WhatsApp booking system is now running with Docker and accessible via ngrok!

