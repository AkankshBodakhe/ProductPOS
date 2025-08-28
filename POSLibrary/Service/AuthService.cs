using System;
using System.Security.Cryptography;
using System.Text;
using POSLibrary.Entities;
using POSLibrary.Repositories;

namespace POSLibrary.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepo = new UserRepository();

        // Register user with hashing
        public string Register(string name, string username, string password, string confirmPassword, string contact)
        {
            if (password != confirmPassword)
                return "Passwords do not match.";

            if (_userRepo.GetByUsername(username) != null)
                return "Username already exists.";

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Name = name,
                Username = username,
                ContactNumber = contact,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            _userRepo.Add(user);
            return "Registration successful!";
        }

        // Login user with password verification
        public User Login(string username, string password)
        {
            var user = _userRepo.GetByUsername(username);
            if (user == null) return null;

            return VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt) ? user : null;
        }

        // -------- Helpers --------
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }
    }
}
