-- =============================================
-- Fix Password Hashes in MediCare_DB
-- Updates all user passwords to use correct hash with salt
-- =============================================

USE MediCare_DB;
GO

-- Update all users with correct password hash for "admin123"
-- Hash: 8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo= (SHA256("admin123MediCare_Salt_Key"))

UPDATE users
SET PasswordHash = '8NYvqGWY+F+VN+A7LeyFN4lZ/jA/UO6Jm5kpfnlO9qo='
WHERE PasswordHash = 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=';

GO

-- Verify the update
SELECT 
    Id, 
    Email, 
    Role, 
    Name, 
    LEFT(PasswordHash, 20) + '...' AS PasswordHash
FROM users
ORDER BY Id;

GO

PRINT '✓ Password hashes updated successfully!';
PRINT 'All users can now login with password: admin123';
GO
