using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AgendamentoApp.Entities;
using AgendamentoApp.Interfaces;

namespace AgendamentoApp.Infraestrutura
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AgendamentoAppContext _context;

        public AdminRepository(AgendamentoAppContext context)
        {
            _context = context;
        }

        public async Task<Admin> GetAdminByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

        public async Task<Admin> GetAdminByEmailAsync(string email)
        {
            return await _context.Admins
                .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Admin>> GetAllAdminsAsync()
        {
            return await _context.Admins.ToListAsync();
        }

        public async Task<bool> AddAsync(Admin admin)
        {
            try
            {
                await _context.Admins.AddAsync(admin);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Admin admin)
        {
            try
            {
                _context.Entry(admin).State = EntityState.Modified;
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
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null) return false;

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Admins.AnyAsync(a => a.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> ValidateCredentialsAsync(string email, string senha)
        {
            var admin = await GetAdminByEmailAsync(email);
            return admin != null && admin.Senha == senha;
        }

        public async Task<bool> ChangeUserPasswordAsync(string userType, int userId, string newPassword)
        {
            try
            {
                switch (userType.ToLower())
                {
                    case "cliente":
                        var cliente = await _context.Clientes.FindAsync(userId);
                        if (cliente != null)
                        {
                            cliente.Senha = newPassword;
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "funcionario":
                        var funcionario = await _context.Funcionarios.FindAsync(userId);
                        if (funcionario != null)
                        {
                            funcionario.Senha = newPassword;
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "admin":
                        var admin = await _context.Admins.FindAsync(userId);
                        if (admin != null)
                        {
                            admin.Senha = newPassword;
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        break;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userType, int userId)
        {
            try
            {
                switch (userType.ToLower())
                {
                    case "cliente":
                        var cliente = await _context.Clientes.FindAsync(userId);
                        if (cliente != null)
                        {
                            _context.Clientes.Remove(cliente);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        break;

                    case "funcionario":
                        var funcionario = await _context.Funcionarios.FindAsync(userId);
                        if (funcionario != null)
                        {
                            _context.Funcionarios.Remove(funcionario);
                            await _context.SaveChangesAsync();
                            return true;
                        }
                        break;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, int>> GetSystemStatisticsAsync()
        {
            var stats = new Dictionary<string, int>
            {
                { "TotalClientes", await _context.Clientes.CountAsync() },
                { "TotalFuncionarios", await _context.Funcionarios.CountAsync() },
                { "TotalServicos", await _context.Servicos.CountAsync() },
                { "AgendamentosHoje", await _context.Agendamentos
                    .CountAsync(a => a.DataHora.Date == DateTime.Today) },
                { "AgendamentosFuturos", await _context.Agendamentos
                    .CountAsync(a => a.DataHora > DateTime.Now) }
            };

            return stats;
        }
    }
}
