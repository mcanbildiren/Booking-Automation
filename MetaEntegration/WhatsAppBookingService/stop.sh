#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

echo -e "${RED}🛑 Stopping WhatsApp Booking Service...${NC}"

docker-compose down

echo -e "${GREEN}✅ All services stopped${NC}"
echo ""
echo "To start again: ./start.sh"

