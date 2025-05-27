using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgendamentoApp.Entities;

namespace AgendamentoApp.Interfaces
{
    public interface IClienteRepository
    {
        Task<Cliente> GetClienteByIdAsync(int id);
        Task<Cliente> GetClienteByEmailAsync(string email);
        Task<IEnumerable<Cliente>> GetAllClientesAsync();
        Task<bool> AddAsync(Cliente cliente);
        Task<bool> UpdateAsync(Cliente cliente);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> ValidateCredentialsAsync(string email, string senha);
    }
}
