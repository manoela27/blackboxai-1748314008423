using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AgendamentoApp.Entities;
using AgendamentoApp.Interfaces;

namespace AgendamentoApp.Infraestrutura
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly AgendamentoAppContext _context;

        public ClienteRepository(AgendamentoAppContext context)
        {
            _context = context;
        }

        public async Task<Cliente> GetClienteByIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<Cliente> GetClienteByEmailAsync(string email)
        {
            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Cliente>> GetAllClientesAsync()
        {
            return await _context.Clientes.ToListAsync();
        }

        public async Task<bool> AddAsync(Cliente cliente)
        {
            try
            {
                await _context.Clientes.AddAsync(cliente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Cliente cliente)
        {
            try
            {
                _context.Entry(cliente).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                    return false;

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Clientes
                .AnyAsync(c => c.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string senha)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

            if (cliente == null)
                return false;

            // Na prática, você deve usar hash da senha
            return cliente.Senha == senha;
        }
    }
}
