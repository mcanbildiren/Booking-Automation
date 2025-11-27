-- Admin Panel SQL Queries for Hairdresser Booking System
-- Use these queries in your admin panel application

-- ============================================
-- DAILY APPOINTMENTS VIEW
-- ============================================

-- Get all appointments for today
SELECT 
    a.id,
    a.appointment_time,
    a.status,
    u.name as customer_name,
    u.phone_number,
    a.duration_minutes,
    a.service_type,
    a.notes
FROM appointments a
JOIN users u ON a.user_id = u.id
WHERE a.appointment_date = CURRENT_DATE
ORDER BY a.appointment_time;

-- Get appointments for a specific date
SELECT 
    a.id,
    a.appointment_time,
    a.status,
    u.name as customer_name,
    u.phone_number,
    a.duration_minutes,
    a.service_type,
    a.notes,
    a.created_at
FROM appointments a
JOIN users u ON a.user_id = u.id
WHERE a.appointment_date = '2025-11-27'  -- Replace with desired date
ORDER BY a.appointment_time;

-- Get appointments for date range
SELECT 
    a.appointment_date,
    a.appointment_time,
    a.status,
    u.name as customer_name,
    u.phone_number,
    a.service_type
FROM appointments a
JOIN users u ON a.user_id = u.id
WHERE a.appointment_date BETWEEN '2025-11-01' AND '2025-11-30'
ORDER BY a.appointment_date, a.appointment_time;


-- ============================================
-- APPOINTMENT STATISTICS
-- ============================================

-- Count appointments by status for today
SELECT 
    status,
    COUNT(*) as count
FROM appointments
WHERE appointment_date = CURRENT_DATE
GROUP BY status;

-- Daily booking count for current month
SELECT 
    appointment_date,
    COUNT(*) as total_bookings,
    COUNT(CASE WHEN status = 'confirmed' THEN 1 END) as confirmed,
    COUNT(CASE WHEN status = 'cancelled' THEN 1 END) as cancelled
FROM appointments
WHERE appointment_date >= DATE_TRUNC('month', CURRENT_DATE)
    AND appointment_date < DATE_TRUNC('month', CURRENT_DATE) + INTERVAL '1 month'
GROUP BY appointment_date
ORDER BY appointment_date;

-- Most popular time slots (last 30 days)
SELECT 
    appointment_time,
    COUNT(*) as booking_count
FROM appointments
WHERE appointment_date >= CURRENT_DATE - INTERVAL '30 days'
    AND status IN ('confirmed', 'completed')
GROUP BY appointment_time
ORDER BY booking_count DESC;


-- ============================================
-- CUSTOMER MANAGEMENT
-- ============================================

-- Get all customers with their appointment count
SELECT 
    u.id,
    u.name,
    u.phone_number,
    u.created_at,
    u.last_contact,
    COUNT(a.id) as total_appointments,
    COUNT(CASE WHEN a.status = 'completed' THEN 1 END) as completed_appointments,
    COUNT(CASE WHEN a.status = 'cancelled' THEN 1 END) as cancelled_appointments,
    MAX(a.appointment_date) as last_appointment_date
FROM users u
LEFT JOIN appointments a ON u.id = a.user_id
GROUP BY u.id, u.name, u.phone_number, u.created_at, u.last_contact
ORDER BY u.last_contact DESC;

-- Get customer details with upcoming appointments
SELECT 
    u.name,
    u.phone_number,
    a.appointment_date,
    a.appointment_time,
    a.status
FROM users u
JOIN appointments a ON u.id = a.user_id
WHERE u.id = 123  -- Replace with customer ID
    AND a.appointment_date >= CURRENT_DATE
ORDER BY a.appointment_date, a.appointment_time;

-- Get new customers (registered in last 7 days)
SELECT 
    name,
    phone_number,
    created_at,
    last_contact
FROM users
WHERE created_at >= CURRENT_DATE - INTERVAL '7 days'
ORDER BY created_at DESC;


-- ============================================
-- UPDATE OPERATIONS
-- ============================================

-- Update appointment status
UPDATE appointments
SET status = 'completed',  -- or 'confirmed', 'cancelled'
    updated_at = NOW()
WHERE id = 123;  -- Replace with appointment ID

-- Add notes to appointment
UPDATE appointments
SET notes = 'Customer requested short haircut',
    service_type = 'Haircut',
    updated_at = NOW()
WHERE id = 123;  -- Replace with appointment ID

-- Mark no-show appointments
UPDATE appointments
SET status = 'cancelled',
    notes = COALESCE(notes || ' | ', '') || 'No-show',
    updated_at = NOW()
WHERE appointment_date < CURRENT_DATE
    AND status = 'confirmed';


-- ============================================
-- WEEKLY/MONTHLY VIEWS
-- ============================================

-- Get current week's appointments
SELECT 
    a.appointment_date,
    a.appointment_time,
    u.name as customer_name,
    u.phone_number,
    a.status
FROM appointments a
JOIN users u ON a.user_id = u.id
WHERE a.appointment_date >= DATE_TRUNC('week', CURRENT_DATE)
    AND a.appointment_date < DATE_TRUNC('week', CURRENT_DATE) + INTERVAL '1 week'
ORDER BY a.appointment_date, a.appointment_time;

-- Get current month's appointments
SELECT 
    a.appointment_date,
    a.appointment_time,
    u.name as customer_name,
    u.phone_number,
    a.status
FROM appointments a
JOIN users u ON a.user_id = u.id
WHERE a.appointment_date >= DATE_TRUNC('month', CURRENT_DATE)
    AND a.appointment_date < DATE_TRUNC('month', CURRENT_DATE) + INTERVAL '1 month'
ORDER BY a.appointment_date, a.appointment_time;


-- ============================================
-- REVENUE & BUSINESS ANALYTICS
-- ============================================

-- Add price column to appointments table (optional)
-- ALTER TABLE appointments ADD COLUMN price DECIMAL(10,2);

-- Daily revenue (if price column exists)
SELECT 
    appointment_date,
    COUNT(*) as total_appointments,
    SUM(price) as total_revenue
FROM appointments
WHERE status = 'completed'
    AND appointment_date >= DATE_TRUNC('month', CURRENT_DATE)
GROUP BY appointment_date
ORDER BY appointment_date;

-- Monthly revenue summary
SELECT 
    DATE_TRUNC('month', appointment_date) as month,
    COUNT(*) as total_appointments,
    COUNT(CASE WHEN status = 'completed' THEN 1 END) as completed,
    COUNT(CASE WHEN status = 'cancelled' THEN 1 END) as cancelled
FROM appointments
WHERE appointment_date >= CURRENT_DATE - INTERVAL '6 months'
GROUP BY DATE_TRUNC('month', appointment_date)
ORDER BY month DESC;


-- ============================================
-- AVAILABILITY CHECKER
-- ============================================

-- Check available slots for a specific date
WITH business_hours AS (
    SELECT generate_series(
        '09:00'::time,
        '17:00'::time,
        '1 hour'::interval
    ) AS time_slot
),
booked_slots AS (
    SELECT appointment_time
    FROM appointments
    WHERE appointment_date = '2025-11-27'  -- Replace with desired date
        AND status IN ('confirmed', 'pending')
)
SELECT 
    bh.time_slot,
    CASE 
        WHEN bs.appointment_time IS NULL THEN 'Available'
        ELSE 'Booked'
    END as status
FROM business_hours bh
LEFT JOIN booked_slots bs ON bh.time_slot = bs.appointment_time
ORDER BY bh.time_slot;


-- ============================================
-- DELETE OPERATIONS (USE WITH CAUTION)
-- ============================================

-- Delete old cancelled appointments (older than 3 months)
DELETE FROM appointments
WHERE status = 'cancelled'
    AND appointment_date < CURRENT_DATE - INTERVAL '3 months';

-- Delete a specific appointment
DELETE FROM appointments
WHERE id = 123;  -- Replace with appointment ID


-- ============================================
-- SEARCH OPERATIONS
-- ============================================

-- Search customer by phone number
SELECT 
    u.id,
    u.name,
    u.phone_number,
    COUNT(a.id) as total_appointments
FROM users u
LEFT JOIN appointments a ON u.id = a.user_id
WHERE u.phone_number LIKE '%555%'  -- Replace with search term
GROUP BY u.id, u.name, u.phone_number;

-- Search customer by name
SELECT 
    u.id,
    u.name,
    u.phone_number,
    u.last_contact
FROM users u
WHERE LOWER(u.name) LIKE LOWER('%john%')  -- Replace with search term
ORDER BY u.last_contact DESC;

-- Search appointments by customer name
SELECT 
    a.id,
    a.appointment_date,
    a.appointment_time,
    a.status,
    u.name as customer_name,
    u.phone_number
FROM appointments a
JOIN users u ON a.user_id = u.id
WHERE LOWER(u.name) LIKE LOWER('%john%')  -- Replace with search term
ORDER BY a.appointment_date DESC, a.appointment_time DESC;

