using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgendamentoApp.Models
{
    public class AgendamentoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A data é obrigatória")]
        [Display(Name = "Data")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; }

        [Required(ErrorMessage = "A hora é obrigatória")]
        [Display(Name = "Hora")]
        [DataType(DataType.Time)]
        public TimeSpan Hora { get; set; }

        [Required(ErrorMessage = "O serviço é obrigatório")]
        [Display(Name = "Serviço")]
        public int ServicoId { get; set; }

        [Required(ErrorMessage = "O profissional é obrigatório")]
        [Display(Name = "Profissional")]
        public int FuncionarioId { get; set; }

        // Propriedades de navegação para exibição
        public string ClienteNome { get; set; }
        public string ServicoNome { get; set; }
        public string FuncionarioNome { get; set; }

        // Listas para os dropdowns
        public IEnumerable<SelectListItem> Servicos { get; set; }
        public IEnumerable<SelectListItem> Funcionarios { get; set; }

        // Propriedade computada para combinar Data e Hora
        public DateTime DataHora
        {
            get => Data.Add(Hora);
            set
            {
                Data = value.Date;
                Hora = value.TimeOfDay;
            }
        }
    }

    public class AgendamentoListViewModel
    {
        public int Id { get; set; }
        public DateTime DataHora { get; set; }
        public string ClienteNome { get; set; }
        public string ServicoNome { get; set; }
        public string FuncionarioNome { get; set; }
        public bool IsFuturo { get; set; }

        public string Status => IsFuturo ? "Agendado" : "Realizado";
    }

    public class DashboardAgendamentoViewModel
    {
        public IEnumerable<AgendamentoListViewModel> AgendamentosFuturos { get; set; }
        public IEnumerable<AgendamentoListViewModel> HistoricoAgendamentos { get; set; }
        public int TotalAgendamentos { get; set; }
        public int AgendamentosHoje { get; set; }
    }

    public class FuncionarioAgendamentosViewModel
    {
        public int FuncionarioId { get; set; }
        public string FuncionarioNome { get; set; }
        public IEnumerable<AgendamentoListViewModel> AgendamentosHoje { get; set; }
        public IEnumerable<AgendamentoListViewModel> ProximosAgendamentos { get; set; }
    }
}
