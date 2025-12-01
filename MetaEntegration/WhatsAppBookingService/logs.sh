#!/bin/bash

# Colors for output
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}📝 Viewing logs (Ctrl+C to exit)...${NC}"
echo ""

docker-compose logs -f whatsapp-service

