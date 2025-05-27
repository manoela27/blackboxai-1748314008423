using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgendamentoApp.Models
{
    public class ServicoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório")]
        [Display(Name = "Nome do Serviço")]
        public string Nome { get; set; }

        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Display(Name = "Profissionais")]
        public List<int> FuncionarioIds { get; set; }

        // Lista de funcionários para o multi-select
        public IEnumerable<SelectListItem> Funcionarios { get; set; }

        // Lista de funcionários já associados ao serviço (para exibição)
        public List<string> FuncionariosAssociados { get; set; }

        public ServicoViewModel()
        {
            FuncionarioIds = new List<int>();
            FuncionariosAssociados = new List<string>();
        }
    }

    public class ServicoListViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int QuantidadeFuncionarios { get; set; }
        public List<string> NomesFuncionarios { get; set; }
    }

    public class ServicoManagementViewModel
    {
        public List<ServicoListViewModel> Servicos { get; set; }
        public ServicoViewModel NovoServico { get; set; }
    }

    public class ServicoFuncionarioViewModel
    {
        public int ServicoId { get; set; }
        public string ServicoNome { get; set; }
        public List<FuncionarioServicoViewModel> Funcionarios { get; set; }
    }

    public class FuncionarioServicoViewModel
    {
        public int FuncionarioId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public bool Selecionado { get; set; }
    }

    public class ServicoDropdownViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public bool Disponivel { get; set; }
    }
}
