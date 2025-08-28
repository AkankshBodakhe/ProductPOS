using System.Linq;
using POSLibrary.Data;
using POSLibrary.Entities;

namespace POSLibrary.Repositories
{
    public class UserRepository
    {
        private readonly POSDbContext _context;

        public UserRepository()
        {
            _context = new POSDbContext();
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public User GetByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }
    }
}
