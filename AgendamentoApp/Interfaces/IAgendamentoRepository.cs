using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgendamentoApp.Entities;

namespace AgendamentoApp.Interfaces
{
    public interface IAgendamentoRepository
    {
        Task<Agendamento> GetAgendamentoByIdAsync(int id);
        Task<IEnumerable<Agendamento>> GetAgendamentosByClienteIdAsync(int clienteId);
        Task<IEnumerable<Agendamento>> GetAgendamentosByFuncionarioIdAsync(int funcionarioId);
        Task<IEnumerable<Agendamento>> GetFuturosAgendamentosByClienteIdAsync(int clienteId);
        Task<IEnumerable<Agendamento>> GetHistoricoAgendamentosByClienteIdAsync(int clienteId);
        Task<IEnumerable<Agendamento>> GetAgendamentosByDataAsync(DateTime data);
        Task<bool> AddAsync(Agendamento agendamento);
        Task<bool> UpdateAsync(Agendamento agendamento);
        Task<bool> DeleteAsync(int id);
        Task<bool> FuncionarioDisponivelAsync(int funcionarioId, DateTime dataHora);
        Task<bool> HorarioDisponivelAsync(int funcionarioId, DateTime dataHora);
        Task<bool> ValidarAgendamentoAsync(Agendamento agendamento);
    }
}
