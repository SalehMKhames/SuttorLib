using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuttorLib.Models.Library;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Data;
using SuttorLibrary.DTOs;
using SuttorLibrary.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SuttorLibrary.Core.Repositories
{
    public class UserRepository(
        AppDbContext context, UserManager<AppUser> userManager
    ) : GenericRepo<AppUser>(context), IUserRepository
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly AppDbContext _context = context;

        public async Task<bool> AddUserInterest(string userId, List<string> categoryNames)
        {
            var isUserExist = await _userManager.FindByIdAsync(userId);
            if (isUserExist == null) 
                throw new KeyNotFoundException("User not found.");

            List<string> categories = new List<string>();
            foreach (var category in categoryNames) 
            {
                var c = await _context.Categories.FindAsync(category);
                if (c == null) continue;

                categories.Add(c.Id.ToString());
            }

            foreach (var id in categories)
            {
                await _context.UserInterests
                    .AddAsync(new UserInterests 
                    {
                        Id = Guid.NewGuid().ToString(), 
                        UserId = userId, 
                        Category_Id = id
                    });

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public override Task<IEnumerable<AppUser>> GetAll()
        {
            return base.GetAll();
        }

        public override Task<AppUser?> GetById (string userid) 
        {
            return base.GetById(userid);
        }

        public async Task<GetPublicUserDTO?> GetUserByEmail(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return null;

            var userDto = new GetPublicUserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = NormalizeUsername(user.UserName),
                Email = user.Email,
                JoinedAt = user.JoinedAt,
                XP = user.XP,
                PhotoPath = user.PhotoPath,
                IsAuthor = user.IsAuthor
            };

            return userDto;
        }

        public async Task<GetPublicUserDTO?> GetUserByUsername(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
                return null;

            var userDto = new GetPublicUserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = NormalizeUsername(user.UserName),
                Email = user.Email,
                JoinedAt = user.JoinedAt,
                XP = user.XP,
                PhotoPath = user.PhotoPath,
                IsAuthor = user.IsAuthor
            };

            return userDto;
        }

        public async Task<bool> PromoteToAuthor(AppUser user, int xp)
        {
            // Add XP
            user.XP += xp;

            // Check if user qualifies for author role
            if (user.XP > 1500 && !user.IsAuthor)
            {
                user.IsAuthor = true;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<List<Category?>?> UpdateUserInterest(string userId, List<string> categoryNames)
        {
            //Find the user
            var isUserExist = await _userManager.FindByIdAsync(userId);
            if (isUserExist is null)
                throw new KeyNotFoundException("User Not Found.");

            var newCategoryIds = new List<string>();
            foreach (var category in categoryNames)
            {
                var cateByName = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase));

                if (cateByName != null)
                    newCategoryIds.Add(cateByName.Id);
                else continue;
            }

            //Get the user's interests' IDs from the Categories Table
            var existingInterests = await _context.UserInterests
               .Where(ui => ui.UserId == userId)
               .ToListAsync();
            var existingIds = existingInterests.Select(ui => ui.Category_Id).ToList();

            // Calculate additions and removals
            var toAdd = newCategoryIds.Except(existingIds).ToList();
            var toRemove = existingInterests.Where(ui => !newCategoryIds.Contains(ui.Category_Id)).ToList();

            // Apply removals
            if (toRemove.Any())
            {
                _context.UserInterests.RemoveRange(toRemove);
            }

            // Apply additions
            foreach (var id in toAdd)
            {
                await _context.UserInterests.AddAsync(new UserInterests
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Category_Id = id
                });
            }

            // Persist changes once
            await _context.SaveChangesAsync();

            // Return the updated category objects for the user (or null if none)
            var updatedCategoryIds = await _context.UserInterests
                .Where(ui => ui.UserId == userId)
                .Select(ui => ui.Category_Id)
                .ToListAsync();

            var userCategories = await _context.Categories
                .Where(c => updatedCategoryIds.Contains(c.Id))
                .ToListAsync();

            return userCategories.Count > 0 ? userCategories.Cast<Category?>().ToList() : null;
        }

        private static string NormalizeUsername(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return string.Empty;
            return username.StartsWith('@') ? username : "@" + username;
        }

        public async Task<List<BookListItemDto>?> SuggestedBooks(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User Token is required");

            var IsExistedUser = await _userManager.FindByIdAsync(userId);
            if (IsExistedUser is null)
                throw new KeyNotFoundException($"User with the ID: {userId} not found");

            //Get the IDs of the categories that the user has interested in.
            var categoriesIDs = await _context.UserInterests
                .Where(ui => ui.UserId == userId)
                .Select (ui => ui.Category_Id)
                .AsNoTracking()
                .ToListAsync();

            //Get 5-10 Books from these categories using the joinig between the Category, Book, BookCategories tables
            var books = new List<Book>();
            foreach (var id in categoriesIDs)
            {
                IQueryable<Book> book = _context.Categories
                    .Where(c => c.Id == id)
                    .Join(
                        _context.BookCategories,
                        c => c.Id,
                        bc => bc.categoryId,
                        (c, bc) => bc)
                    .Join(
                        _context.Books,
                        bc => bc.bookId,
                        b => b.Id,
                        (bc, b) => b)
                    .Distinct()
                    .AsNoTracking();

                books.AddRange(book.Take(5));
            }

            var items = books
                .Select(b => new BookListItemDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    PageCount = b.PageCount,
                    PublishedAT = b.PublishedAT,
                    FilePath = b.FilePath,
                    FileSize = b.FileSize,
                    PhotoPath = b.PhotoPath,
                    UploadedAt = b.UploadedAt,
                    Language = _context.Languages
                        .Where(l => l.Id == b.LanguageId)
                        .Select(l => l.Language)
                        .FirstOrDefault() ?? string.Empty,
                    Authors_Names = _context.BookAuthors
                        .Where(ba => ba.Book_Id == b.Id)
                        .Join(
                            _context.Authors,
                            ba => ba.Author_Id,
                            a => a.Id,
                            (ba, a) => a.Name)
                        .ToList(),
                    Categories_Names = _context.BookCategories
                        .Where(bc => bc.bookId == b.Id)
                        .Join(
                            _context.Categories,
                            bc => bc.categoryId,
                            c => c.Id,
                            (bc, c) => c.Name)
                        .ToList()
                }).ToList();

            return items;
        }
    }
}
