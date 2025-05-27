using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AgendamentoApp.Entities;
using AgendamentoApp.Interfaces;

namespace AgendamentoApp.Infraestrutura
{
    public class FuncionarioRepository : IFuncionarioRepository
    {
        private readonly AgendamentoAppContext _context;

        public FuncionarioRepository(AgendamentoAppContext context)
        {
            _context = context;
        }

        public async Task<Funcionario> GetFuncionarioByIdAsync(int id)
        {
            return await _context.Funcionarios
                .Include(f => f.Servicos)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Funcionario> GetFuncionarioByEmailAsync(string email)
        {
            return await _context.Funcionarios
                .Include(f => f.Servicos)
                .FirstOrDefaultAsync(f => f.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Funcionario>> GetAllFuncionariosAsync()
        {
            return await _context.Funcionarios
                .Include(f => f.Servicos)
                .ToListAsync();
        }

        public async Task<IEnumerable<Funcionario>> GetFuncionariosByServicoIdAsync(int servicoId)
        {
            return await _context.Funcionarios
                .Include(f => f.Servicos)
                .Where(f => f.Servicos.Any(s => s.Id == servicoId))
                .ToListAsync();
        }

        public async Task<bool> AddAsync(Funcionario funcionario)
        {
            try
            {
                await _context.Funcionarios.AddAsync(funcionario);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Funcionario funcionario)
        {
            try
            {
                _context.Entry(funcionario).State = EntityState.Modified;
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
            var funcionario = await _context.Funcionarios.FindAsync(id);
            if (funcionario == null) return false;

            _context.Funcionarios.Remove(funcionario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Funcionarios.AnyAsync(f => f.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string senha)
        {
            var funcionario = await GetFuncionarioByEmailAsync(email);
            return funcionario != null && funcionario.Senha == senha;
        }

        public async Task<bool> AddServicoToFuncionarioAsync(int funcionarioId, int servicoId)
        {
            var funcionario = await _context.Funcionarios
                .Include(f => f.Servicos)
                .FirstOrDefaultAsync(f => f.Id == funcionarioId);
            
            var servico = await _context.Servicos.FindAsync(servicoId);
            
            if (funcionario == null || servico == null) return false;
            
            funcionario.Servicos.Add(servico);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveServicoFromFuncionarioAsync(int funcionarioId, int servicoId)
        {
            var funcionario = await _context.Funcionarios
                .Include(f => f.Servicos)
                .FirstOrDefaultAsync(f => f.Id == funcionarioId);
            
            var servico = await _context.Servicos.FindAsync(servicoId);
            
            if (funcionario == null || servico == null) return false;
            
            funcionario.Servicos.Remove(servico);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
