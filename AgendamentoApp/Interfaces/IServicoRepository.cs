using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgendamentoApp.Entities;

namespace AgendamentoApp.Interfaces
{
    public interface IServicoRepository
    {
        Task<Servico> GetServicoByIdAsync(int id);
        Task<IEnumerable<Servico>> GetAllServicosAsync();
        Task<IEnumerable<Servico>> GetServicosByFuncionarioIdAsync(int funcionarioId);
        Task<bool> AddAsync(Servico servico);
        Task<bool> UpdateAsync(Servico servico);
        Task<bool> DeleteAsync(int id);
        Task<bool> ServicoExistsAsync(string nome);
        Task<bool> AssignFuncionarioToServicoAsync(int servicoId, int funcionarioId);
        Task<bool> RemoveFuncionarioFromServicoAsync(int servicoId, int funcionarioId);
        Task<IEnumerable<Funcionario>> GetAvailableFuncionariosForServicoAsync(int servicoId, DateTime dataHora);
    }
}
