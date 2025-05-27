using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using AgendamentoApp.Models;
using AgendamentoApp.Interfaces;
using AgendamentoApp.Entities;

namespace AgendamentoApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IAgendamentoRepository _agendamentoRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public ClienteController(
            IClienteRepository clienteRepository,
            IAgendamentoRepository agendamentoRepository,
            IServicoRepository servicoRepository,
            IFuncionarioRepository funcionarioRepository)
        {
            _clienteRepository = clienteRepository;
            _agendamentoRepository = agendamentoRepository;
            _servicoRepository = servicoRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        public async Task<IActionResult> Dashboard()
        {
            var clienteId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var agendamentosFuturos = await _agendamentoRepository.GetFuturosAgendamentosByClienteIdAsync(clienteId);
            var historicoAgendamentos = await _agendamentoRepository.GetHistoricoAgendamentosByClienteIdAsync(clienteId);

            var viewModel = new DashboardAgendamentoViewModel
            {
                AgendamentosFuturos = agendamentosFuturos.Select(a => new AgendamentoListViewModel
                {
                    Id = a.Id,
                    DataHora = a.DataHora,
                    ServicoNome = a.Servico.Nome,
                    FuncionarioNome = a.Funcionario.Nome,
                    IsFuturo = true
                }),
                HistoricoAgendamentos = historicoAgendamentos.Select(a => new AgendamentoListViewModel
                {
                    Id = a.Id,
                    DataHora = a.DataHora,
                    ServicoNome = a.Servico.Nome,
                    FuncionarioNome = a.Funcionario.Nome,
                    IsFuturo = false
                }),
                TotalAgendamentos = agendamentosFuturos.Count() + historicoAgendamentos.Count(),
                AgendamentosHoje = agendamentosFuturos.Count(a => a.DataHora.Date == DateTime.Today)
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> NovoAgendamento()
        {
            var servicos = await _servicoRepository.GetAllServicosAsync();
            
            var viewModel = new AgendamentoViewModel
            {
                Data = DateTime.Today,
                Hora = new TimeSpan(9, 0, 0), // Começa às 9h
                Servicos = servicos.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Nome
                })
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovoAgendamento(AgendamentoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var clienteId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                
                var agendamento = new Agendamento
                {
                    ClienteId = clienteId,
                    ServicoId = model.ServicoId,
                    FuncionarioId = model.FuncionarioId,
                    DataHora = model.DataHora
                };

                if (await _agendamentoRepository.AddAsync(agendamento))
                {
                    return RedirectToAction(nameof(Dashboard));
                }

                ModelState.AddModelError(string.Empty, "Erro ao criar agendamento. Verifique a disponibilidade.");
            }

            // Recarrega as listas em caso de erro
            var servicos = await _servicoRepository.GetAllServicosAsync();
            model.Servicos = servicos.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Nome
            });

            if (model.ServicoId > 0)
            {
                var funcionarios = await _funcionarioRepository.GetFuncionariosByServicoIdAsync(model.ServicoId);
                model.Funcionarios = funcionarios.Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Nome
                });
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetFuncionarios(int servicoId, DateTime dataHora)
        {
            var funcionariosDisponiveis = await _servicoRepository
                .GetAvailableFuncionariosForServicoAsync(servicoId, dataHora);

            var funcionariosList = funcionariosDisponiveis.Select(f => new
            {
                id = f.Id,
                nome = f.Nome
            });

            return Json(funcionariosList);
        }

        public async Task<IActionResult> EditarPerfil()
        {
            var clienteId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var cliente = await _clienteRepository.GetClienteByIdAsync(clienteId);

            if (cliente == null)
                return NotFound();

            return View(new RegisterViewModel
            {
                Nome = cliente.Nome,
                Email = cliente.Email,
                Endereco = cliente.Endereco
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var clienteId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
                var cliente = await _clienteRepository.GetClienteByIdAsync(clienteId);

                if (cliente == null)
                    return NotFound();

                cliente.Nome = model.Nome;
                cliente.Endereco = model.Endereco;

                if (await _clienteRepository.UpdateAsync(cliente))
                {
                    TempData["StatusMessage"] = "Perfil atualizado com sucesso.";
                    return RedirectToAction(nameof(Dashboard));
                }

                ModelState.AddModelError(string.Empty, "Erro ao atualizar perfil.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirConta()
        {
            var clienteId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            
            if (await _clienteRepository.DeleteAsync(clienteId))
            {
                await HttpContext.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = "Erro ao excluir conta. Tente novamente.";
            return RedirectToAction(nameof(EditarPerfil));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAgendamento(int id)
        {
            var clienteId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var agendamento = await _agendamentoRepository.GetAgendamentoByIdAsync(id);

            if (agendamento == null || agendamento.ClienteId != clienteId)
                return NotFound();

            if (agendamento.DataHora <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Não é possível cancelar agendamentos passados.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (await _agendamentoRepository.DeleteAsync(id))
            {
                TempData["StatusMessage"] = "Agendamento cancelado com sucesso.";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao cancelar agendamento.";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
