using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using WholesalerManager.Core.Domain.IdentityEntities;
using WholesalerManager.Core.Domain.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace WholesalerManager.Infrastructure.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResult> AddNewRoleToUser(ApplicationUser user, string roleName)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> AddUser(ApplicationUser user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> DeleteUser(Guid id)
        {
            var matchingUser = _userManager.Users.FirstOrDefault(u => u.Id == id);
            if (matchingUser is null)
            {
                return IdentityResult.Failed();
            }
            IdentityResult result = await _userManager.DeleteAsync(matchingUser);
            return result;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid id)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<ApplicationUser?> GetUserByNameAsync(string name)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == name);
        }

        public async Task<IdentityResult> UpdateUser(ApplicationUser user)
        {
            ApplicationUser? matchingUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (matchingUser is null)
            {
                return IdentityResult.Failed();
            }

            matchingUser.FirstName = user.FirstName;
            matchingUser.LastName = user.LastName;
            matchingUser.UserName = user.UserName;
            matchingUser.Email = user.Email;
            matchingUser.PhoneNumber = user.PhoneNumber;

            IdentityResult result = await _userManager.UpdateAsync(matchingUser);
            return result;
        }
    }
}
