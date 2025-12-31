-- ============================================
-- MediCare Pvt Ltd Healthcare System
-- Database Setup Script for SQL Server
-- ============================================

-- Step 1: Create Database
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'MediCare_DB')
BEGIN
    ALTER DATABASE MediCare_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MediCare_DB;
END
GO

CREATE DATABASE MediCare_DB;
GO

USE MediCare_DB;
GO

PRINT '✓ Database created successfully';
GO

-- ============================================
-- Step 2: Create Tables
-- ============================================

-- Users Table
CREATE TABLE users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    DateOfBirth DATE NULL,
    ContactNumber NVARCHAR(15) NULL,
    Gender NVARCHAR(10) NULL,
    BloodGroup NVARCHAR(10) NULL,
    Address NVARCHAR(255) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT DEFAULT 1
);
PRINT '✓ Users table created';
GO

-- Doctors Table
CREATE TABLE doctors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Specialty NVARCHAR(100) NOT NULL,
    Qualifications NVARCHAR(500) NULL,
    LicenseNumber NVARCHAR(10) NULL,
    ConsultationFee DECIMAL(10,2) DEFAULT 0,
    Availability NVARCHAR(500) NULL,
    Bio NVARCHAR(1000) NULL,
    ProfileImage NVARCHAR(255) NULL,
    ExperienceYears INT DEFAULT 0,
    Location NVARCHAR(100) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT DEFAULT 1
);
PRINT '✓ Doctors table created';
GO

-- Patients Table
CREATE TABLE patients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(15) NULL,
    Birthdate DATE NULL,
    Address NVARCHAR(255) NULL,
    EmergencyContact NVARCHAR(50) NULL,
    EmergencyContactPhone NVARCHAR(100) NULL,
    MedicalHistory NVARCHAR(500) NULL,
    Allergies NVARCHAR(500) NULL,
    BloodType NVARCHAR(10) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL,
    IsActive BIT DEFAULT 1
);
PRINT '✓ Patients table created';
GO

-- Appointments Table
CREATE TABLE appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    Notes NVARCHAR(1000) NULL,
    Prescription NVARCHAR(1000) NULL,
    Diagnosis NVARCHAR(500) NULL,
    Fee DECIMAL(10,2) NULL,
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending',
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);
PRINT '✓ Appointments table created';
GO

-- Payments Table
CREATE TABLE payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    PatientId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    TransactionId NVARCHAR(100) NULL,
    PaymentGateway NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CompletedAt DATETIME2 NULL
);
PRINT '✓ Payments table created';
GO

-- Feedbacks Table
CREATE TABLE feedbacks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentId INT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Rating INT NOT NULL,
    Title NVARCHAR(100) NULL,
    IsApproved BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);
PRINT '✓ Feedbacks table created';
GO

-- DoctorPatients Junction Table
CREATE TABLE DoctorPatients (
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    PRIMARY KEY (DoctorId, PatientId)
);
PRINT '✓ DoctorPatients junction table created';
GO

-- ============================================
-- Step 3: Insert Sample Data
-- ============================================

PRINT 'Inserting sample data...';
GO

-- Sample Users (Password: admin123)
INSERT INTO users (Email, PasswordHash, Role, Name, ContactNumber, Gender, IsActive)
VALUES 
    ('admin@medicare.lk', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Admin', 'System Administrator', '+94711234567', 'Male', 1),
    ('doctor1@medicare.lk', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Doctor', 'Dr. John Smith', '+94711234568', 'Male', 1),
    ('doctor2@medicare.lk', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Doctor', 'Dr. Sarah Johnson', '+94711234570', 'Female', 1),
    ('doctor3@medicare.lk', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Doctor', 'Dr. Michael Brown', '+94711234571', 'Male', 1),
    ('doctor4@medicare.lk', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Doctor', 'Dr. Emily Davis', '+94711234572', 'Female', 1),
    ('patient@medicare.lk', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Patient', 'Jane Doe', '+94711234569', 'Female', 1);
PRINT '✓ Sample users inserted';
GO

-- Sample Doctors
INSERT INTO doctors (UserId, Name, Specialty, Qualifications, ConsultationFee, ExperienceYears, Location, Bio, IsActive)
VALUES 
    (2, 'Dr. John Smith', 'General Medicine', 'MBBS, MD', 1500.00, 10, 'Colombo', 'Experienced general practitioner specializing in family medicine and preventive care.', 1),
    (3, 'Dr. Sarah Johnson', 'Pediatrics', 'MBBS, DCH', 1800.00, 8, 'Colombo', 'Specialist in child health and development with expertise in neonatal care.', 1),
    (4, 'Dr. Michael Brown', 'Cardiology', 'MBBS, MD, DM (Cardiology)', 2500.00, 15, 'Kandy', 'Renowned cardiologist with extensive experience in interventional cardiology.', 1),
    (5, 'Dr. Emily Davis', 'Dermatology', 'MBBS, MD (Dermatology)', 2000.00, 12, 'Colombo', 'Expert in treating skin conditions, cosmetic dermatology, and laser treatments.', 1);
PRINT '✓ Sample doctors inserted';
GO

-- Sample Patient
INSERT INTO patients (UserId, Name, Email, Phone, Address, BloodType, IsActive)
VALUES 
    (6, 'Jane Doe', 'patient@medicare.lk', '+94711234569', '123 Main Street, Colombo 07, Sri Lanka', 'O+', 1);
PRINT '✓ Sample patient inserted';
GO

-- ============================================
-- Step 4: Create Sample Appointments
-- ============================================

-- Sample Appointments
INSERT INTO appointments (DoctorId, PatientId, AppointmentDate, AppointmentTime, Status, Fee, PaymentStatus)
VALUES 
    (1, 1, DATEADD(day, 1, GETDATE()), '09:00:00', 'Confirmed', 1500.00, 'Pending'),
    (2, 1, DATEADD(day, 2, GETDATE()), '10:30:00', 'Pending', 1800.00, 'Pending'),
    (3, 1, DATEADD(day, 3, GETDATE()), '14:00:00', 'Pending', 2500.00, 'Pending');
PRINT '✓ Sample appointments inserted';
GO

-- ============================================
-- Step 5: Verify Database Setup
-- ============================================

PRINT '';
PRINT '============================================';
PRINT 'DATABASE SETUP COMPLETED SUCCESSFULLY!';
PRINT '============================================';
PRINT '';

-- Display summary
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM users
UNION ALL
SELECT 'Doctors' AS TableName, COUNT(*) AS RecordCount FROM doctors
UNION ALL
SELECT 'Patients' AS TableName, COUNT(*) AS RecordCount FROM patients
UNION ALL
SELECT 'Appointments' AS TableName, COUNT(*) AS RecordCount FROM appointments
UNION ALL
SELECT 'Payments' AS TableName, COUNT(*) AS RecordCount FROM payments
UNION ALL
SELECT 'Feedbacks' AS TableName, COUNT(*) AS RecordCount FROM feedbacks
UNION ALL
SELECT 'DoctorPatients' AS TableName, COUNT(*) AS RecordCount FROM DoctorPatients;

PRINT '';
PRINT '============================================';
PRINT 'LOGIN CREDENTIALS:';
PRINT '============================================';
PRINT 'Admin:   admin@medicare.lk / admin123';
PRINT 'Doctor:  doctor1@medicare.lk / admin123';
PRINT 'Patient: patient@medicare.lk / admin123';
PRINT '============================================';
PRINT '';

-- Display sample data
PRINT 'Users:';
SELECT Id, Email, Role, Name, ContactNumber, IsActive FROM users;

PRINT '';
PRINT 'Doctors:';
SELECT Id, Name, Specialty, ConsultationFee, ExperienceYears, Location FROM doctors;

PRINT '';
PRINT 'Patients:';
SELECT Id, Name, Email, Phone, BloodType FROM patients;

PRINT '';
PRINT 'Appointments:';
SELECT Id, DoctorId, PatientId, AppointmentDate, AppointmentTime, Status, Fee FROM appointments;

GO
