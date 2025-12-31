-- =============================================
-- MediCare Healthcare System - Simple SQL Setup
-- SQL Server Database with Dummy Data
-- =============================================

-- Create Database
CREATE DATABASE MediCare_DB;
GO

USE MediCare_DB;
GO

-- =============================================
-- CREATE TABLES
-- =============================================

-- Users Table
CREATE TABLE users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    DateOfBirth DATE,
    ContactNumber NVARCHAR(15),
    Gender NVARCHAR(10),
    BloodGroup NVARCHAR(10),
    Address NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsActive BIT DEFAULT 1
);

-- Doctors Table
CREATE TABLE doctors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Specialty NVARCHAR(100) NOT NULL,
    Qualifications NVARCHAR(500),
    LicenseNumber NVARCHAR(10),
    ConsultationFee DECIMAL(10,2) DEFAULT 0,
    Availability NVARCHAR(500),
    Bio NVARCHAR(1000),
    ProfileImage NVARCHAR(255),
    ExperienceYears INT DEFAULT 0,
    Location NVARCHAR(100),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsActive BIT DEFAULT 1
);

-- Patients Table
CREATE TABLE patients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(15),
    Birthdate DATE,
    Address NVARCHAR(255),
    EmergencyContact NVARCHAR(50),
    EmergencyContactPhone NVARCHAR(100),
    MedicalHistory NVARCHAR(500),
    Allergies NVARCHAR(500),
    BloodType NVARCHAR(10),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2,
    IsActive BIT DEFAULT 1
);

-- Appointments Table
CREATE TABLE appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    Notes NVARCHAR(1000),
    Prescription NVARCHAR(1000),
    Diagnosis NVARCHAR(500),
    Fee DECIMAL(10,2),
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending',
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2
);

-- Payments Table
CREATE TABLE payments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    PatientId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending',
    TransactionId NVARCHAR(100),
    PaymentGateway NVARCHAR(100),
    Description NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CompletedAt DATETIME2
);

-- Feedbacks Table
CREATE TABLE feedbacks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentId INT,
    Message NVARCHAR(1000) NOT NULL,
    Rating INT NOT NULL,
    Title NVARCHAR(100),
    IsApproved BIT DEFAULT 0,
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- DoctorPatients Table
CREATE TABLE DoctorPatients (
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    PRIMARY KEY (DoctorId, PatientId)
);

GO

-- =============================================
-- INSERT DUMMY DATA
-- =============================================

-- Password for all users: admin123
-- Hash: 8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo= (SHA256 with salt "MediCare_Salt_Key")

-- Insert Admin User
INSERT INTO users (Email, PasswordHash, Role, Name, ContactNumber, Gender, BloodGroup, Address, IsActive)
VALUES ('admin@medicare.lk', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Admin', 'System Administrator', '+94112345678', 'Male', 'A+', '456 Admin Street, Colombo 03', 1);

-- Insert Doctor Users
INSERT INTO users (Email, PasswordHash, Role, Name, DateOfBirth, ContactNumber, Gender, BloodGroup, Address, IsActive)
VALUES 
    ('john.smith@medicare.lk', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Doctor', 'Dr. John Smith', '1980-05-15', '+94771234001', 'Male', 'O+', '123 Medical Lane, Colombo 07', 1),
    ('sarah.johnson@medicare.lk', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Doctor', 'Dr. Sarah Johnson', '1985-08-22', '+94771234002', 'Female', 'B+', '234 Health Avenue, Colombo 05', 1),
    ('michael.brown@medicare.lk', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Doctor', 'Dr. Michael Brown', '1975-03-10', '+94771234003', 'Male', 'AB+', '345 Care Road, Kandy', 1),
    ('emily.davis@medicare.lk', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Doctor', 'Dr. Emily Davis', '1988-11-30', '+94771234004', 'Female', 'A-', '456 Wellness Street, Galle', 1),
    ('david.wilson@medicare.lk', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Doctor', 'Dr. David Wilson', '1982-07-18', '+94771234005', 'Male', 'O-', '567 Clinic Way, Colombo 04', 1);

-- Insert Patient Users
INSERT INTO users (Email, PasswordHash, Role, Name, DateOfBirth, ContactNumber, Gender, BloodGroup, Address, IsActive)
VALUES 
    ('patient1@gmail.com', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Patient', 'Jane Doe', '1990-04-25', '+94771111001', 'Female', 'O+', '789 Patient Road, Colombo 08', 1),
    ('patient2@gmail.com', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Patient', 'Robert Anderson', '1985-09-14', '+94771111002', 'Male', 'A+', '890 Home Street, Negombo', 1),
    ('patient3@gmail.com', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Patient', 'Maria Garcia', '1995-12-05', '+94771111003', 'Female', 'B+', '901 Family Lane, Kandy', 1),
    ('patient4@gmail.com', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Patient', 'James Taylor', '1978-06-20', '+94771111004', 'Male', 'AB-', '012 Health Avenue, Gampaha', 1),
    ('patient5@gmail.com', '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo=', 'Patient', 'Lisa Martinez', '2000-02-10', '+94771111005', 'Female', 'O-', '123 Care Street, Matara', 1);

GO

-- Insert Doctor Profiles
INSERT INTO doctors (UserId, Name, Specialty, Qualifications, LicenseNumber, ConsultationFee, Availability, Bio, ExperienceYears, Location, IsActive)
VALUES 
    (2, 'Dr. John Smith', 'General Medicine', 'MBBS, MD (General Medicine)', 'SLMC12001', 1500.00, 'Mon-Fri: 9AM-5PM', 'Experienced general practitioner with over 10 years of experience in family medicine and preventive healthcare.', 10, 'Colombo', 1),
    (3, 'Dr. Sarah Johnson', 'Pediatrics', 'MBBS, DCH, MD (Pediatrics)', 'SLMC12002', 1800.00, 'Mon-Sat: 10AM-6PM', 'Specialist in child healthcare with expertise in neonatal care and child development.', 8, 'Colombo', 1),
    (4, 'Dr. Michael Brown', 'Cardiology', 'MBBS, MD, DM (Cardiology)', 'SLMC12003', 2500.00, 'Mon-Thu: 2PM-8PM', 'Renowned cardiologist specializing in interventional cardiology and heart disease management.', 15, 'Kandy', 1),
    (5, 'Dr. Emily Davis', 'Dermatology', 'MBBS, MD (Dermatology)', 'SLMC12004', 2000.00, 'Tue-Sat: 9AM-5PM', 'Expert dermatologist offering treatment for skin conditions, cosmetic procedures, and laser treatments.', 12, 'Galle', 1),
    (6, 'Dr. David Wilson', 'Orthopedics', 'MBBS, MS (Orthopedics)', 'SLMC12005', 2200.00, 'Mon-Fri: 8AM-4PM', 'Orthopedic surgeon with specialization in sports injuries and joint replacement surgery.', 14, 'Colombo', 1);

GO

-- Insert Patient Profiles
INSERT INTO patients (UserId, Name, Email, Phone, Birthdate, Address, EmergencyContact, EmergencyContactPhone, MedicalHistory, Allergies, BloodType, IsActive)
VALUES 
    (7, 'Jane Doe', 'patient1@gmail.com', '+94771111001', '1990-04-25', '789 Patient Road, Colombo 08', 'John Doe', '+94771111101', 'Hypertension', 'Penicillin', 'O+', 1),
    (8, 'Robert Anderson', 'patient2@gmail.com', '+94771111002', '1985-09-14', '890 Home Street, Negombo', 'Mary Anderson', '+94771111102', 'Diabetes Type 2', 'None', 'A+', 1),
    (9, 'Maria Garcia', 'patient3@gmail.com', '+94771111003', '1995-12-05', '901 Family Lane, Kandy', 'Carlos Garcia', '+94771111103', 'Asthma', 'Sulfa drugs', 'B+', 1),
    (10, 'James Taylor', 'patient4@gmail.com', '+94771111004', '1978-06-20', '012 Health Avenue, Gampaha', 'Susan Taylor', '+94771111104', 'Heart disease', 'Aspirin', 'AB-', 1),
    (11, 'Lisa Martinez', 'patient5@gmail.com', '+94771111005', '2000-02-10', '123 Care Street, Matara', 'Rosa Martinez', '+94771111105', 'None', 'None', 'O-', 1);

GO

-- Insert Appointments
INSERT INTO appointments (DoctorId, PatientId, AppointmentDate, AppointmentTime, Status, Notes, Fee, PaymentStatus)
VALUES 
    (1, 1, '2025-11-18', '09:00:00', 'Confirmed', 'Regular checkup', 1500.00, 'Paid'),
    (1, 2, '2025-11-18', '10:00:00', 'Pending', 'Follow-up consultation', 1500.00, 'Pending'),
    (2, 3, '2025-11-19', '11:00:00', 'Confirmed', 'Child vaccination', 1800.00, 'Paid'),
    (3, 4, '2025-11-20', '14:00:00', 'Pending', 'Cardiac evaluation', 2500.00, 'Pending'),
    (4, 5, '2025-11-21', '10:30:00', 'Confirmed', 'Skin consultation', 2000.00, 'Paid'),
    (5, 1, '2025-11-22', '09:00:00', 'Pending', 'Knee pain evaluation', 2200.00, 'Pending'),
    (1, 3, '2025-11-23', '15:00:00', 'Confirmed', 'General consultation', 1500.00, 'Paid'),
    (2, 2, '2025-11-24', '10:00:00', 'Pending', 'Pediatric checkup', 1800.00, 'Pending');

GO

-- Insert Payments
INSERT INTO payments (AppointmentId, PatientId, Amount, PaymentMethod, Status, TransactionId, PaymentGateway, Description, CompletedAt)
VALUES 
    (1, 1, 1500.00, 'Card', 'Completed', 'TXN001234567', 'Visa', 'Payment for appointment on 2025-11-18', '2025-11-16 10:30:00'),
    (3, 3, 1800.00, 'Online', 'Completed', 'TXN001234568', 'PayPal', 'Payment for vaccination appointment', '2025-11-16 11:45:00'),
    (5, 5, 2000.00, 'Card', 'Completed', 'TXN001234569', 'MasterCard', 'Dermatology consultation payment', '2025-11-16 14:20:00'),
    (7, 1, 1500.00, 'Cash', 'Completed', 'CASH001', 'Cash', 'Cash payment at clinic', '2025-11-16 15:00:00');

GO

-- Insert Feedbacks
INSERT INTO feedbacks (PatientId, DoctorId, AppointmentId, Message, Rating, Title, IsApproved)
VALUES 
    (1, 1, 1, 'Dr. Smith is very professional and caring. He explained everything clearly and took time to answer all my questions.', 5, 'Excellent Doctor', 1),
    (3, 2, 3, 'Dr. Johnson is wonderful with children. My daughter felt comfortable and the treatment was very effective.', 5, 'Great Pediatrician', 1),
    (5, 4, 5, 'Very knowledgeable dermatologist. The treatment plan was effective and I saw results quickly.', 4, 'Good Experience', 1),
    (2, 1, NULL, 'Good service and professional staff. Wait time was minimal.', 4, 'Satisfied Patient', 1);

GO

-- Insert Doctor-Patient Relationships
INSERT INTO DoctorPatients (DoctorId, PatientId)
VALUES 
    (1, 1), (1, 2), (1, 3),
    (2, 2), (2, 3),
    (3, 4),
    (4, 5),
    (5, 1);

GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

PRINT '✓ Database setup completed successfully!';
PRINT '';
PRINT '====================================';
PRINT 'DATABASE STATISTICS';
PRINT '====================================';

SELECT 'Users' AS [Table], COUNT(*) AS [Records] FROM users
UNION ALL
SELECT 'Doctors', COUNT(*) FROM doctors
UNION ALL
SELECT 'Patients', COUNT(*) FROM patients
UNION ALL
SELECT 'Appointments', COUNT(*) FROM appointments
UNION ALL
SELECT 'Payments', COUNT(*) FROM payments
UNION ALL
SELECT 'Feedbacks', COUNT(*) FROM feedbacks;

PRINT '';
PRINT '====================================';
PRINT 'LOGIN CREDENTIALS (Password: admin123)';
PRINT '====================================';
PRINT 'Admin:    admin@medicare.lk';
PRINT 'Doctor 1: john.smith@medicare.lk';
PRINT 'Doctor 2: sarah.johnson@medicare.lk';
PRINT 'Patient 1: patient1@gmail.com';
PRINT 'Patient 2: patient2@gmail.com';
PRINT '====================================';

GO

-- Show sample data
SELECT 'USERS' AS Info;
SELECT Id, Email, Role, Name, ContactNumber FROM users;

SELECT 'DOCTORS' AS Info;
SELECT Id, Name, Specialty, ConsultationFee, ExperienceYears, Location FROM doctors;

SELECT 'PATIENTS' AS Info;
SELECT Id, Name, Email, Phone, BloodType FROM patients;

SELECT 'APPOINTMENTS' AS Info;
SELECT Id, DoctorId, PatientId, AppointmentDate, AppointmentTime, Status, Fee FROM appointments;

SELECT 'PAYMENTS' AS Info;
SELECT Id, AppointmentId, Amount, PaymentMethod, Status, TransactionId FROM payments;

SELECT 'FEEDBACKS' AS Info;
SELECT Id, PatientId, DoctorId, Rating, Title FROM feedbacks;

GO
