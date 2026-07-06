using Microsoft.AspNetCore.Identity;
using SuttorLib.Core.Services.Notifications;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Core.Repositories;
using SuttorLibrary.Data;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core
{
    public class UnitOfWork(AppDbContext context, UserManager<AppUser> userManager, IConfiguration config) 
        : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context = context;
        private UserManager<AppUser> _userManager = userManager;
        protected readonly IConfiguration _config = config;

        private IAuthRepository? _authRepo { get; set; }
        private IUserRepository? _userRepo { get; set; }
        private IBookRepository? _bookRepo { get; set; }
        private IFCMRepository? _FCMRepo { get; set; }

        public IAuthRepository AuthRepo
        {
            get { return _authRepo ??= new AuthRepository(_context, _userManager, _config); }
        }

        public IUserRepository UserRepo
        {
            get { return _userRepo ??= new UserRepository(_context, _userManager); }
        }

        public IBookRepository BookRepo
        {
            get { return _bookRepo ??= new BookRepository(_context); }
        }

        public IFCMRepository FCMRepo 
        {
            get { return _FCMRepo ??= new FCMRepository(_context); }
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
