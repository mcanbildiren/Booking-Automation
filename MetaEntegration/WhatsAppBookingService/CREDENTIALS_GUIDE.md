# 🔑 Complete Credentials Setup Guide

This guide will walk you through getting all the credentials you need.

---

## 1️⃣ ngrok Auth Token (5 minutes)

### Steps:

1. **Sign up for ngrok** (Free account)
   - Go to: https://dashboard.ngrok.com/signup
   - Sign up with email or GitHub

2. **Get your auth token**
   - After signing in, you'll see your dashboard
   - Go to: https://dashboard.ngrok.com/get-started/your-authtoken
   - Click **"Copy"** next to your authtoken
   
   It looks like: `2abcdefghijklmnop_1234567890ABCDEFGHIJKLMN`

3. **Save it** - You'll need this for your `.env` file

---

## 2️⃣ Meta WhatsApp Business API Credentials (15 minutes)

### A. Create/Access Meta Developer Account

1. **Go to Meta for Developers**
   - Visit: https://developers.facebook.com
   - Click **"Get Started"** (top right)
   - Log in with your Facebook account

2. **Create a New App** (or select existing one)
   - Click **"My Apps"** → **"Create App"**
   - Select type: **"Business"**
   - Give it a name: e.g., "Hairdresser Booking Bot"
   - Add your email
   - Click **"Create App"**

### B. Add WhatsApp Product

1. **In your app dashboard**
   - Find **"WhatsApp"** in the products list
   - Click **"Set Up"**

2. **You'll be in the WhatsApp Quickstart**
   - This shows API Setup section

### C. Get Your Credentials

#### ✅ Credential 1: Phone Number ID

1. In **"WhatsApp" → "API Setup"** page
2. Look for **"From"** dropdown/field
3. You'll see a test phone number (provided by Meta)
4. Click on the info icon or check the number details
5. **Copy the Phone Number ID**
   
   Example: `123456789012345` (15 digits)

#### ✅ Credential 2: Access Token (Temporary for Testing)

1. On the same page, find **"Temporary access token"**
2. Click **"Copy"** 
   
   Example: `EAADZBbJ4OyTwBOxxxxxxxxxxxxxxxxxxxxxx`

**⚠️ Important:** This token expires in 24 hours. For production, you'll need a permanent token (see below).

#### ✅ Credential 3: Verify Token (You Create This!)

This is YOUR secret string. Create something secure:

Examples:
- `hairdresser_webhook_secret_2024_xyz123`
- `my_super_secret_verify_token_abc456`

**Remember this!** You'll use it in both:
- Your `.env` file
- Meta webhook configuration

### D. Create Permanent Access Token (For Production)

**For testing, you can skip this. But for production:**

1. Go to **Meta Business Settings**
   - Visit: https://business.facebook.com/settings
   
2. **Create System User**
   - Click **"Users"** → **"System Users"**
   - Click **"Add"** → Create a system user
   - Name: "WhatsApp API User"
   - Role: Admin

3. **Generate Token**
   - Click on the system user
   - Click **"Generate New Token"**
   - Select your app
   - Permissions: Select **"whatsapp_business_messaging"**
   - Click **"Generate Token"**
   - **Copy and save this token** - it won't expire!

---

## 3️⃣ Add Test Phone Number (Required for Testing)

1. **In WhatsApp API Setup page**
   - Scroll to **"To"** field
   - Click **"Add phone number"**
   - Enter YOUR phone number (the one you'll test with)
   - Click **"Send Code"**

2. **Verify your phone**
   - You'll receive a WhatsApp message with a code
   - Enter the code
   - Click **"Verify"**

**✅ Now you can receive messages from the test number!**

---

## 4️⃣ Fill in Your .env File

Now that you have all credentials, create your `.env` file:

```bash
cd /Users/mcanbildiren/Documents/Repos/BookingSystem/MetaEntegration/WhatsAppBookingService

# Copy example
cp env.example .env

# Edit it
nano .env
```

**Fill in with your actual values:**

```bash
# Database (you can keep this default)
DB_PASSWORD=postgres123

# WhatsApp API Credentials
WHATSAPP_PHONE_NUMBER_ID=123456789012345              # From Step 2C
WHATSAPP_ACCESS_TOKEN=EAADZBbJ4OyTwBOxxxxxxxxx        # From Step 2C
WHATSAPP_VERIFY_TOKEN=hairdresser_webhook_secret_2024  # YOU created this

# ngrok Credential
NGROK_AUTHTOKEN=2abcdefghijklmnop_1234567890ABCDEF    # From Step 1
```

Save the file (Ctrl+O, Enter, Ctrl+X in nano)

---

## ✅ Verification Checklist

Before running `./start.sh`, make sure you have:

- [ ] ngrok account created
- [ ] ngrok auth token copied
- [ ] Meta Developer account created
- [ ] WhatsApp product added to your app
- [ ] Phone Number ID copied
- [ ] Access Token copied
- [ ] Your own Verify Token created
- [ ] Your test phone number added and verified in Meta
- [ ] `.env` file created and filled with all credentials

---

## 🚀 Next Steps

Once all credentials are in `.env`:

```bash
# Start everything
./start.sh

# You'll get your webhook URL like:
# https://abc123.ngrok.io/api/webhook
```

Then configure the webhook in Meta (covered in DOCKER_SETUP.md)

---

## 🔍 Finding Your Credentials Again

### ngrok Token
https://dashboard.ngrok.com/get-started/your-authtoken

### WhatsApp Credentials
1. Go to: https://developers.facebook.com
2. Click **"My Apps"** → Select your app
3. Click **"WhatsApp"** in left menu
4. Go to **"API Setup"**

---

## ❓ Common Issues

### "I can't find Phone Number ID"
- Go to: WhatsApp → API Setup
- Look for the test number under "From"
- The Phone Number ID is shown below the number or in the number details

### "My access token expired"
- Temporary tokens expire in 24 hours
- Either get a new temporary token
- Or create a permanent System User token (Step 2D)

### "I didn't verify my phone number"
- You must add and verify YOUR phone number in Meta
- Go to: WhatsApp → API Setup → "To" field
- Add your number and verify with the code sent via WhatsApp

### "ngrok shows invalid token"
- Double-check you copied the full token
- No spaces before/after in .env file
- Token looks like: `2abc...` (starts with number)

---

## 📞 Support Links

- **ngrok Dashboard**: https://dashboard.ngrok.com
- **Meta Developers**: https://developers.facebook.com
- **WhatsApp Cloud API Docs**: https://developers.facebook.com/docs/whatsapp/cloud-api
- **Meta Business Settings**: https://business.facebook.com/settings

---

**Once you have all credentials in `.env`, run `./start.sh` and you're ready to test! 🎉**

