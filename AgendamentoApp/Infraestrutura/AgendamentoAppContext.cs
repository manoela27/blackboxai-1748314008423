using System;
using System.Data.Entity;
using AgendamentoApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendamentoApp.Infraestrutura
{
    public class AgendamentoAppContext : DbContext
    {
        public AgendamentoAppContext(DbContextOptions<AgendamentoAppContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do relacionamento many-to-many entre Servico e Funcionario
            modelBuilder.Entity<Servico>()
                .HasMany(s => s.Funcionarios)
                .WithMany(f => f.Servicos)
                .UsingEntity(j => j.ToTable("ServicoFuncionario"));

            // Configuração das chaves estrangeiras do Agendamento
            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Cliente)
                .WithMany()
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Funcionario)
                .WithMany()
                .HasForeignKey(a => a.FuncionarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Servico)
                .WithMany()
                .HasForeignKey(a => a.ServicoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para melhor performance
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Funcionario>()
                .HasIndex(f => f.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }
    }
}
