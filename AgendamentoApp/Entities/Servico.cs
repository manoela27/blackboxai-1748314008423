using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AgendamentoApp.Entities
{
    public class Servico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório")]
        public string Nome { get; set; }

        public string Descricao { get; set; }

        // Relacionamento many-to-many com Funcionarios
        public virtual ICollection<Funcionario> Funcionarios { get; set; }

        public Servico()
        {
            Funcionarios = new HashSet<Funcionario>();
        }
    }
}
