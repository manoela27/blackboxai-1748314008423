using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AgendamentoApp.Models;
using AgendamentoApp.Interfaces;

namespace AgendamentoApp.Controllers
{
    [Authorize(Roles = "Funcionario")]
    public class FuncionarioController : Controller
    {
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IServicoRepository _servicoRepository;

        public FuncionarioController(
            IFuncionarioRepository funcionarioRepository,
            IAgendamentoRepository agendamentoRepository,
            IServicoRepository servicoRepository)
        {
            _funcionarioRepository = funcionarioRepository;
            _agendamentoRepository = agendamentoRepository;
            _servicoRepository = servicoRepository;
        }

        public async Task<IActionResult> Dashboard()
        {
            var funcionarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            
            var agendamentosHoje = await _agendamentoRepository.GetAgendamentosByDataAsync(DateTime.Today);
            var proximosAgendamentos = await _agendamentoRepository.GetAgendamentosByFuncionarioIdAsync(funcionarioId);

            var viewModel = new FuncionarioAgendamentosViewModel
            {
                FuncionarioId = funcionarioId,
                FuncionarioNome = User.Identity.Name,
                AgendamentosHoje = agendamentosHoje
                    .Where(a => a.FuncionarioId == funcionarioId)
                    .Select(a => new AgendamentoListViewModel
                    {
                        Id = a.Id,
                        DataHora = a.DataHora,
                        ClienteNome = a.Cliente.Nome,
                        ServicoNome = a.Servico.Nome,
                        IsFuturo = a.DataHora > DateTime.Now
                    }),
                ProximosAgendamentos = proximosAgendamentos
                    .Where(a => a.DataHora > DateTime.Today)
                    .OrderBy(a => a.DataHora)
                    .Select(a => new AgendamentoListViewModel
                    {
                        Id = a.Id,
                        DataHora = a.DataHora,
                        ClienteNome = a.Cliente.Nome,
                        ServicoNome = a.Servico.Nome,
                        IsFuturo = true
                    })
            };

            return View(viewModel);
        }

        public async Task<IActionResult> MeusServicos()
        {
            var funcionarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var servicos = await _servicoRepository.GetServicosByFuncionarioIdAsync(funcionarioId);

            var viewModel = servicos.Select(s => new ServicoListViewModel
            {
                Id = s.Id,
                Nome = s.Nome,
                Descricao = s.Descricao
            });

            return View(viewModel);
        }

        public async Task<IActionResult> Perfil()
        {
            var funcionarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(funcionarioId);

            if (funcionario == null)
                return NotFound();

            var viewModel = new RegisterViewModel
            {
                Nome = funcionario.Nome,
                Email = funcionario.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtualizarPerfil(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var funcionarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(funcionarioId);

                if (funcionario == null)
                    return NotFound();

                funcionario.Nome = model.Nome;

                if (await _funcionarioRepository.UpdateAsync(funcionario))
                {
                    TempData["StatusMessage"] = "Perfil atualizado com sucesso.";
                    return RedirectToAction(nameof(Dashboard));
                }

                ModelState.AddModelError(string.Empty, "Erro ao atualizar perfil.");
            }

            return View("Perfil", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenha(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var funcionarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                var funcionario = await _funcionarioRepository.GetFuncionarioByIdAsync(funcionarioId);

                if (funcionario == null)
                    return NotFound();

                if (funcionario.Senha != model.SenhaAtual)
                {
                    ModelState.AddModelError(string.Empty, "Senha atual incorreta.");
                    return View("Perfil", new RegisterViewModel { Nome = funcionario.Nome, Email = funcionario.Email });
                }

                funcionario.Senha = model.NovaSenha;

                if (await _funcionarioRepository.UpdateAsync(funcionario))
                {
                    TempData["StatusMessage"] = "Senha alterada com sucesso.";
                    return RedirectToAction(nameof(Dashboard));
                }

                ModelState.AddModelError(string.Empty, "Erro ao alterar senha.");
            }

            var viewModel = new RegisterViewModel
            {
                Nome = User.Identity.Name,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
            };

            return View("Perfil", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AgendamentosPorData(DateTime data)
        {
            var funcionarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var agendamentos = await _agendamentoRepository.GetAgendamentosByDataAsync(data);

            var agendamentosList = agendamentos
                .Where(a => a.FuncionarioId == funcionarioId)
                .Select(a => new AgendamentoListViewModel
                {
                    Id = a.Id,
                    DataHora = a.DataHora,
                    ClienteNome = a.Cliente.Nome,
                    ServicoNome = a.Servico.Nome,
                    IsFuturo = a.DataHora > DateTime.Now
                });

            return Json(agendamentosList);
        }
    }
}
