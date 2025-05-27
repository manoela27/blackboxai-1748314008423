using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AgendamentoApp.Entities;
using AgendamentoApp.Interfaces;

namespace AgendamentoApp.Infraestrutura
{
    public class AgendamentoRepository : IAgendamentoRepository
    {
        private readonly AgendamentoAppContext _context;

        public AgendamentoRepository(AgendamentoAppContext context)
        {
            _context = context;
        }

        public async Task<Agendamento> GetAgendamentoByIdAsync(int id)
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Funcionario)
                .Include(a => a.Servico)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Agendamento>> GetAgendamentosByClienteIdAsync(int clienteId)
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Funcionario)
                .Include(a => a.Servico)
                .Where(a => a.ClienteId == clienteId)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Agendamento>> GetAgendamentosByFuncionarioIdAsync(int funcionarioId)
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Funcionario)
                .Include(a => a.Servico)
                .Where(a => a.FuncionarioId == funcionarioId)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Agendamento>> GetFuturosAgendamentosByClienteIdAsync(int clienteId)
        {
            var dataAtual = DateTime.Now;
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Funcionario)
                .Include(a => a.Servico)
                .Where(a => a.ClienteId == clienteId && a.DataHora > dataAtual)
                .OrderBy(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Agendamento>> GetHistoricoAgendamentosByClienteIdAsync(int clienteId)
        {
            var dataAtual = DateTime.Now;
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Funcionario)
                .Include(a => a.Servico)
                .Where(a => a.ClienteId == clienteId && a.DataHora <= dataAtual)
                .OrderByDescending(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<IEnumerable<Agendamento>> GetAgendamentosByDataAsync(DateTime data)
        {
            return await _context.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Funcionario)
                .Include(a => a.Servico)
                .Where(a => a.DataHora.Date == data.Date)
                .OrderBy(a => a.DataHora)
                .ToListAsync();
        }

        public async Task<bool> AddAsync(Agendamento agendamento)
        {
            try
            {
                if (!await ValidarAgendamentoAsync(agendamento))
                    return false;

                await _context.Agendamentos.AddAsync(agendamento);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Agendamento agendamento)
        {
            try
            {
                if (!await ValidarAgendamentoAsync(agendamento))
                    return false;

                _context.Entry(agendamento).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento == null) return false;

            _context.Agendamentos.Remove(agendamento);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FuncionarioDisponivelAsync(int funcionarioId, DateTime dataHora)
        {
            return !await _context.Agendamentos
                .AnyAsync(a => a.FuncionarioId == funcionarioId && 
                              a.DataHora == dataHora);
        }

        public async Task<bool> HorarioDisponivelAsync(int funcionarioId, DateTime dataHora)
        {
            return await FuncionarioDisponivelAsync(funcionarioId, dataHora);
        }

        public async Task<bool> ValidarAgendamentoAsync(Agendamento agendamento)
        {
            if (!agendamento.IsDataValida())
                return false;

            // Verifica se o funcionário está disponível
            if (!await FuncionarioDisponivelAsync(agendamento.FuncionarioId, agendamento.DataHora))
                return false;

            // Verifica se o funcionário está associado ao serviço
            var funcionario = await _context.Funcionarios
                .Include(f => f.Servicos)
                .FirstOrDefaultAsync(f => f.Id == agendamento.FuncionarioId);

            if (funcionario == null || !funcionario.Servicos.Any(s => s.Id == agendamento.ServicoId))
                return false;

            return true;
        }
    }
}
