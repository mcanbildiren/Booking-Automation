-- Migration: Add Workers and Worker Schedules
-- Run this script to add worker support to your booking system

-- Create workers table
CREATE TABLE IF NOT EXISTS workers (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    specialty VARCHAR(100),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW()
);

-- Create index on workers
CREATE INDEX IF NOT EXISTS idx_workers_active ON workers(is_active);
CREATE INDEX IF NOT EXISTS idx_workers_name ON workers(name);

-- Create worker_schedules table
CREATE TABLE IF NOT EXISTS worker_schedules (
    id SERIAL PRIMARY KEY,
    worker_id INTEGER REFERENCES workers(id) ON DELETE CASCADE,
    day_of_week INTEGER NOT NULL CHECK (day_of_week >= 0 AND day_of_week <= 6), -- 0 = Sunday, 6 = Saturday
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    is_working BOOLEAN DEFAULT true,
    CONSTRAINT unique_worker_day UNIQUE (worker_id, day_of_week)
);

-- Create index on worker_schedules
CREATE INDEX IF NOT EXISTS idx_worker_schedules_worker ON worker_schedules(worker_id);
CREATE INDEX IF NOT EXISTS idx_worker_schedules_day ON worker_schedules(day_of_week);

-- Add worker_id column to appointments table
ALTER TABLE appointments 
ADD COLUMN IF NOT EXISTS worker_id INTEGER REFERENCES workers(id) ON DELETE CASCADE;

-- Drop the old unique constraint on appointments (date + time only)
ALTER TABLE appointments DROP CONSTRAINT IF EXISTS unique_appointment;

-- Create new unique constraint (worker + date + time)
-- This allows different workers to have appointments at the same time
ALTER TABLE appointments 
ADD CONSTRAINT unique_worker_appointment UNIQUE (worker_id, appointment_date, appointment_time);

-- Create index on appointments for worker
CREATE INDEX IF NOT EXISTS idx_appointments_worker ON appointments(worker_id);

-- =====================================================
-- INSERT SAMPLE WORKERS (Modify as needed)
-- =====================================================

-- Insert sample workers
INSERT INTO workers (name, specialty, is_active) VALUES
('Ahmet', 'Erkek Kuaförü', true),
('Ayşe', 'Kadın Kuaförü', true),
('Mehmet', 'Saç Tasarımı', true)
ON CONFLICT DO NOTHING;

-- Insert sample schedules for each worker (Monday to Saturday, 09:00 - 18:00)
-- Worker 1 (Ahmet) - Works Mon-Sat
INSERT INTO worker_schedules (worker_id, day_of_week, start_time, end_time, is_working)
SELECT 1, day, '09:00'::TIME, '18:00'::TIME, true
FROM generate_series(1, 6) AS day -- Monday (1) to Saturday (6)
ON CONFLICT (worker_id, day_of_week) DO NOTHING;

-- Worker 2 (Ayşe) - Works Mon-Fri
INSERT INTO worker_schedules (worker_id, day_of_week, start_time, end_time, is_working)
SELECT 2, day, '10:00'::TIME, '19:00'::TIME, true
FROM generate_series(1, 5) AS day -- Monday (1) to Friday (5)
ON CONFLICT (worker_id, day_of_week) DO NOTHING;

-- Worker 3 (Mehmet) - Works Tue-Sat
INSERT INTO worker_schedules (worker_id, day_of_week, start_time, end_time, is_working)
SELECT 3, day, '09:00'::TIME, '17:00'::TIME, true
FROM generate_series(2, 6) AS day -- Tuesday (2) to Saturday (6)
ON CONFLICT (worker_id, day_of_week) DO NOTHING;

-- =====================================================
-- VERIFY MIGRATION
-- =====================================================

-- Show workers
SELECT * FROM workers;

-- Show schedules
SELECT 
    w.name as worker_name,
    ws.day_of_week,
    CASE ws.day_of_week 
        WHEN 0 THEN 'Sunday'
        WHEN 1 THEN 'Monday'
        WHEN 2 THEN 'Tuesday'
        WHEN 3 THEN 'Wednesday'
        WHEN 4 THEN 'Thursday'
        WHEN 5 THEN 'Friday'
        WHEN 6 THEN 'Saturday'
    END as day_name,
    ws.start_time,
    ws.end_time,
    ws.is_working
FROM worker_schedules ws
JOIN workers w ON ws.worker_id = w.id
ORDER BY w.name, ws.day_of_week;

