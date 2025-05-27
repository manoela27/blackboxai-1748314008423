using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AgendamentoApp.Entities;
using AgendamentoApp.Interfaces;

namespace AgendamentoApp.Infraestrutura
{
    public class ServicoRepository : IServicoRepository
    {
        private readonly AgendamentoAppContext _context;

        public ServicoRepository(AgendamentoAppContext context)
        {
            _context = context;
        }

        public async Task<Servico> GetServicoByIdAsync(int id)
        {
            return await _context.Servicos
                .Include(s => s.Funcionarios)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Servico>> GetAllServicosAsync()
        {
            return await _context.Servicos
                .Include(s => s.Funcionarios)
                .ToListAsync();
        }

        public async Task<IEnumerable<Servico>> GetServicosByFuncionarioIdAsync(int funcionarioId)
        {
            return await _context.Servicos
                .Include(s => s.Funcionarios)
                .Where(s => s.Funcionarios.Any(f => f.Id == funcionarioId))
                .ToListAsync();
        }

        public async Task<bool> AddAsync(Servico servico)
        {
            try
            {
                await _context.Servicos.AddAsync(servico);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Servico servico)
        {
            try
            {
                _context.Entry(servico).State = EntityState.Modified;
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
            var servico = await _context.Servicos.FindAsync(id);
            if (servico == null) return false;

            _context.Servicos.Remove(servico);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ServicoExistsAsync(string nome)
        {
            return await _context.Servicos.AnyAsync(s => s.Nome.ToLower() == nome.ToLower());
        }

        public async Task<bool> AssignFuncionarioToServicoAsync(int servicoId, int funcionarioId)
        {
            var servico = await _context.Servicos
                .Include(s => s.Funcionarios)
                .FirstOrDefaultAsync(s => s.Id == servicoId);
            
            var funcionario = await _context.Funcionarios.FindAsync(funcionarioId);
            
            if (servico == null || funcionario == null) return false;
            
            servico.Funcionarios.Add(funcionario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFuncionarioFromServicoAsync(int servicoId, int funcionarioId)
        {
            var servico = await _context.Servicos
                .Include(s => s.Funcionarios)
                .FirstOrDefaultAsync(s => s.Id == servicoId);
            
            var funcionario = await _context.Funcionarios.FindAsync(funcionarioId);
            
            if (servico == null || funcionario == null) return false;
            
            servico.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Funcionario>> GetAvailableFuncionariosForServicoAsync(int servicoId, DateTime dataHora)
        {
            return await _context.Funcionarios
                .Include(f => f.Servicos)
                .Where(f => f.Servicos.Any(s => s.Id == servicoId))
                .Where(f => !_context.Agendamentos.Any(a => 
                    a.FuncionarioId == f.Id && 
                    a.DataHora == dataHora))
                .ToListAsync();
        }
    }
}
