using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SuttorLibrary.Core.Interfaces;
using SuttorLibrary.Core.Services;
using SuttorLibrary.Data;
using SuttorLibrary.DTOs;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Repositories
{
    public class UserRepository(
        AppDbContext context, UserManager<AppUser> userManager
    ) : GenericRepo<AppUser>(context), IUserRepository
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly AppDbContext _context = context;

        public async Task<bool?> AddUserInterest(UserInterestDTO dto)
        {
            var isUserExist = await _userManager.FindByIdAsync(dto.UserID);
            if (isUserExist == null) 
                return null;

            List<Guid> categories = new List<Guid>();
            foreach (var category in dto.CategoriesNames) {
                var c = await _context.Categories.FindAsync(category);
                if (c == null) continue;
                categories.Add(c.Id);
            }

            foreach (var id in categories)
            {
                await _context.UserInterests
                    .AddAsync(new UserInterests {Id = Guid.NewGuid(), UserId = dto.UserID, Category_Id = id});

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
                UserName = user.UserName,
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
                UserName = user.UserName,
                Email = user.Email,
                JoinedAt = user.JoinedAt,
                XP = user.XP,
                PhotoPath = user.PhotoPath,
                IsAuthor = user.IsAuthor
            };

            return userDto;
        }

        public async Task<List<Category?>?> UpdateUserInterest(UserInterestDTO dto)
        {
            //Find the user
            var isUserExist = await _userManager.FindByIdAsync(dto.UserID);
            if (isUserExist == null)
                return null;

            var newCategoryIds = new List<Guid>();
            foreach (var category in dto.CategoriesNames)
            {
                if (Guid.TryParse(category, out var parsedGuid))
                {
                    var cate = await _context.Categories.FindAsync(parsedGuid);
                    if (cate != null) newCategoryIds.Add(cate.Id);
                }
                else
                {
                    var cateByName = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
                    if (cateByName != null) newCategoryIds.Add(cateByName.Id);
                }
            }

            //Get the user's new interests' IDs from the Categories Table
            var existingInterests = await _context.UserInterests
               .Where(ui => ui.UserId == dto.UserID)
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
                    Id = Guid.NewGuid(),
                    UserId = dto.UserID,
                    Category_Id = id
                });
            }

            // Persist changes once
            await _context.SaveChangesAsync();

            // Return the updated category objects for the user (or null if none)
            var updatedCategoryIds = await _context.UserInterests
                .Where(ui => ui.UserId == dto.UserID)
                .Select(ui => ui.Category_Id)
                .ToListAsync();

            var userCategories = await _context.Categories
                .Where(c => updatedCategoryIds.Contains(c.Id))
                .ToListAsync();

            return userCategories.Count > 0 ? userCategories.Cast<Category?>().ToList() : null;
        }
    }
}
