using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgendamentoApp.Entities;

namespace AgendamentoApp.Interfaces
{
    public interface IAdminRepository
    {
        Task<Admin> GetAdminByIdAsync(int id);
        Task<Admin> GetAdminByEmailAsync(string email);
        Task<IEnumerable<Admin>> GetAllAdminsAsync();
        Task<bool> AddAsync(Admin admin);
        Task<bool> UpdateAsync(Admin admin);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> ValidateCredentialsAsync(string email, string senha);
        
        // Métodos específicos para gerenciamento de usuários
        Task<bool> ChangeUserPasswordAsync(string userType, int userId, string newPassword);
        Task<bool> DeleteUserAsync(string userType, int userId);
        Task<Dictionary<string, int>> GetSystemStatisticsAsync();
    }
}
