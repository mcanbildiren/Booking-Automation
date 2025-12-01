#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 Starting WhatsApp Booking Service with Docker${NC}"
echo ""

# Check if .env file exists
if [ ! -f .env ]; then
    echo -e "${YELLOW}⚠️  .env file not found. Creating from template...${NC}"
    cp env.example .env
    echo -e "${RED}❌ Please edit .env file with your credentials before continuing!${NC}"
    echo ""
    echo "Required credentials:"
    echo "  1. WHATSAPP_PHONE_NUMBER_ID (from Meta Developer Console)"
    echo "  2. WHATSAPP_ACCESS_TOKEN (from Meta Developer Console)"
    echo "  3. WHATSAPP_VERIFY_TOKEN (create your own secret string)"
    echo "  4. NGROK_AUTHTOKEN (from https://dashboard.ngrok.com)"
    echo ""
    echo "Edit the .env file, then run this script again: ./start.sh"
    exit 1
fi

# Load .env file
source .env

# Check if required variables are set
if [ -z "$WHATSAPP_PHONE_NUMBER_ID" ] || [ "$WHATSAPP_PHONE_NUMBER_ID" = "your_phone_number_id_here" ]; then
    echo -e "${RED}❌ WHATSAPP_PHONE_NUMBER_ID not set in .env${NC}"
    exit 1
fi

if [ -z "$WHATSAPP_ACCESS_TOKEN" ] || [ "$WHATSAPP_ACCESS_TOKEN" = "your_access_token_here" ]; then
    echo -e "${RED}❌ WHATSAPP_ACCESS_TOKEN not set in .env${NC}"
    exit 1
fi

if [ -z "$NGROK_AUTHTOKEN" ] || [ "$NGROK_AUTHTOKEN" = "your_ngrok_authtoken_here" ]; then
    echo -e "${RED}❌ NGROK_AUTHTOKEN not set in .env${NC}"
    echo "Get your token from: https://dashboard.ngrok.com/get-started/your-authtoken"
    exit 1
fi

echo -e "${GREEN}✅ Configuration validated${NC}"
echo ""

# Stop existing containers
echo -e "${YELLOW}🛑 Stopping existing containers...${NC}"
docker-compose down

# Build and start services
echo -e "${YELLOW}🏗️  Building and starting services...${NC}"
docker-compose up -d --build

# Wait for services to be ready
echo -e "${YELLOW}⏳ Waiting for services to start...${NC}"
sleep 10

# Check if containers are running
if [ "$(docker-compose ps -q whatsapp-service)" ] && [ "$(docker-compose ps -q ngrok)" ]; then
    echo -e "${GREEN}✅ All services started successfully!${NC}"
    echo ""
    
    # Get ngrok URL
    echo -e "${GREEN}🌐 Getting your ngrok webhook URL...${NC}"
    sleep 3
    
    NGROK_URL=$(curl -s http://localhost:4040/api/tunnels | grep -o '"public_url":"https://[^"]*' | grep -o 'https://[^"]*' | head -1)
    
    if [ ! -z "$NGROK_URL" ]; then
        WEBHOOK_URL="${NGROK_URL}/api/webhook"
        echo ""
        echo -e "${GREEN}========================================${NC}"
        echo -e "${GREEN}✅ Your Webhook URL is:${NC}"
        echo -e "${YELLOW}${WEBHOOK_URL}${NC}"
        echo -e "${GREEN}========================================${NC}"
        echo ""
        echo "📋 Next steps:"
        echo "1. Go to: https://developers.facebook.com"
        echo "2. Navigate to: Your App → WhatsApp → Configuration"
        echo "3. Click 'Edit' next to Webhook"
        echo "4. Enter:"
        echo "   - Callback URL: ${WEBHOOK_URL}"
        echo "   - Verify Token: ${WHATSAPP_VERIFY_TOKEN}"
        echo "5. Click 'Verify and Save'"
        echo "6. Subscribe to: messages"
        echo ""
        echo "🎉 Then send '/randevu' to your WhatsApp Business number to test!"
        echo ""
    else
        echo -e "${RED}⚠️  Could not retrieve ngrok URL${NC}"
        echo "Check ngrok status at: http://localhost:4040"
    fi
    
    echo -e "${YELLOW}📊 Service URLs:${NC}"
    echo "  - WhatsApp Service: http://localhost:5000"
    echo "  - ngrok Dashboard: http://localhost:4040"
    echo "  - PostgreSQL: localhost:5432"
    echo ""
    echo -e "${YELLOW}📝 Useful commands:${NC}"
    echo "  - View logs: docker-compose logs -f"
    echo "  - Stop services: docker-compose down"
    echo "  - Restart: ./start.sh"
    echo ""
else
    echo -e "${RED}❌ Some services failed to start${NC}"
    echo "Check logs with: docker-compose logs"
    exit 1
fi

