-- Hairdresser Booking System Database Schema
-- PostgreSQL Database Setup

-- Create users table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    phone_number VARCHAR(20) UNIQUE NOT NULL,
    name VARCHAR(100),
    created_at TIMESTAMP DEFAULT NOW(),
    last_contact TIMESTAMP DEFAULT NOW(),
    CONSTRAINT unique_phone UNIQUE (phone_number)
);

-- Create index on phone_number for faster lookups
CREATE INDEX idx_users_phone ON users(phone_number);

-- Create appointments table
CREATE TABLE IF NOT EXISTS appointments (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
    appointment_date DATE NOT NULL,
    appointment_time TIME NOT NULL,
    duration_minutes INTEGER DEFAULT 60,
    status VARCHAR(20) DEFAULT 'pending',
    service_type VARCHAR(100),
    notes TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    CONSTRAINT unique_appointment UNIQUE (appointment_date, appointment_time),
    CONSTRAINT valid_status CHECK (status IN ('pending', 'confirmed', 'cancelled', 'completed'))
);

-- Create indexes for faster queries
CREATE INDEX idx_appointments_date ON appointments(appointment_date);
CREATE INDEX idx_appointments_user ON appointments(user_id);
CREATE INDEX idx_appointments_status ON appointments(status);
CREATE INDEX idx_appointments_date_time ON appointments(appointment_date, appointment_time);

-- Create a trigger to update the updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_appointments_updated_at
    BEFORE UPDATE ON appointments
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Optional: Create a view for today's appointments
CREATE OR REPLACE VIEW todays_appointments AS
SELECT 
    a.id,
    a.appointment_time,
    a.duration_minutes,
    a.status,
    a.service_type,
    u.name as customer_name,
    u.phone_number
FROM appointments a
JOIN users u ON a.user_id = u.id
WHERE a.appointment_date = CURRENT_DATE
ORDER BY a.appointment_time;

-- Optional: Insert some sample working hours configuration
CREATE TABLE IF NOT EXISTS business_config (
    id SERIAL PRIMARY KEY,
    config_key VARCHAR(50) UNIQUE NOT NULL,
    config_value VARCHAR(100) NOT NULL,
    description TEXT
);

INSERT INTO business_config (config_key, config_value, description) VALUES
('business_start_hour', '9', 'Business opening hour (24-hour format)'),
('business_end_hour', '18', 'Business closing hour (24-hour format)'),
('slot_duration_minutes', '60', 'Default appointment duration in minutes'),
('advance_booking_days', '30', 'How many days in advance customers can book')
ON CONFLICT (config_key) DO NOTHING;

-- Grant necessary permissions (adjust username as needed)
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO your_n8n_user;
-- GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO your_n8n_user;

