# Hairdresser Booking System

A WhatsApp-based appointment booking system for hairdressers and salons.

## Overview

This project provides a complete booking solution that allows customers to book appointments via WhatsApp and gives salon owners an admin panel to manage those appointments.

## Components

**n8n Workflow** - Handles WhatsApp messages and manages bookings automatically
- Customers send `/randevu` to see available time slots
- System responds with available hours
- Customers reply with preferred time to book
- Appointments are saved to database

**ASP.NET Core Admin Panel** - Web interface for salon owners
- View daily appointments
- Update appointment status
- Add notes for customers
- See statistics

**PostgreSQL Database** - Stores customer information and appointments

## Features

- ✅ WhatsApp Business integration
- ✅ Real-time availability checking
- ✅ Automatic appointment confirmation
- ✅ Appointment cancellation support
- ✅ Admin dashboard for managing bookings
- ✅ Turkish language support

## Tech Stack

- n8n for workflow automation
- ASP.NET Core MVC for admin panel
- PostgreSQL for database
- WhatsApp Business API

## Setup

1. Create PostgreSQL database using `database_schema.sql`
2. Import `workflow.json` into n8n and configure credentials
3. Set up admin panel by copying `appsettings.example.json` to `appsettings.json` and adding your credentials
4. Run the admin panel with `dotnet run`

## License

Private use

