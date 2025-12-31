using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string password = "admin123";
        string salt = "MediCare_Salt_Key";
        
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        string hash = Convert.ToBase64String(hashedBytes);
        
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {hash}");
    }
}
