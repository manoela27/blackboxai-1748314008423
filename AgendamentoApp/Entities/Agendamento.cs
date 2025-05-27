using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgendamentoApp.Entities
{
    public class Agendamento
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias")]
        [Display(Name = "Data e Hora")]
        public DateTime DataHora { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int ServicoId { get; set; }

        [Required]
        public int FuncionarioId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("ServicoId")]
        public virtual Servico Servico { get; set; }

        [ForeignKey("FuncionarioId")]
        public virtual Funcionario Funcionario { get; set; }

        // Validação customizada para garantir que a data seja futura
        public bool IsDataValida()
        {
            return DataHora > DateTime.Now;
        }
    }
}
