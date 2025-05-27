using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgendamentoApp.Entities;

namespace AgendamentoApp.Interfaces
{
    public interface IFuncionarioRepository
    {
        Task<Funcionario> GetFuncionarioByIdAsync(int id);
        Task<Funcionario> GetFuncionarioByEmailAsync(string email);
        Task<IEnumerable<Funcionario>> GetAllFuncionariosAsync();
        Task<IEnumerable<Funcionario>> GetFuncionariosByServicoIdAsync(int servicoId);
        Task<bool> AddAsync(Funcionario funcionario);
        Task<bool> UpdateAsync(Funcionario funcionario);
        Task<bool> DeleteAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> ValidateCredentialsAsync(string email, string senha);
        Task<bool> AddServicoToFuncionarioAsync(int funcionarioId, int servicoId);
        Task<bool> RemoveServicoFromFuncionarioAsync(int funcionarioId, int servicoId);
    }
}
