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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;

        public AdminController(
            IAdminRepository adminRepository,
            IClienteRepository clienteRepository,
            IFuncionarioRepository funcionarioRepository,
            IServicoRepository servicoRepository)
        {
            _adminRepository = adminRepository;
            _clienteRepository = clienteRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
        }

        public async Task<IActionResult> Dashboard()
        {
            var stats = await _adminRepository.GetSystemStatisticsAsync();
            return View(stats);
        }

        #region Gerenciamento de Usuários

        public async Task<IActionResult> GerenciarUsuarios()
        {
            var clientes = await _clienteRepository.GetAllClientesAsync();
            var funcionarios = await _funcionarioRepository.GetAllFuncionariosAsync();

            var viewModel = new
            {
                Clientes = clientes.Select(c => new
                {
                    c.Id,
                    c.Nome,
                    c.Email,
                    c.Endereco,
                    Tipo = "Cliente"
                }),
                Funcionarios = funcionarios.Select(f => new
                {
                    f.Id,
                    f.Nome,
                    f.Email,
                    Tipo = "Funcionario"
                })
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarSenhaUsuario(string tipo, int userId, string novaSenha)
        {
            if (string.IsNullOrEmpty(novaSenha))
            {
                return Json(new { success = false, message = "A nova senha não pode estar vazia." });
            }

            var resultado = await _adminRepository.ChangeUserPasswordAsync(tipo, userId, novaSenha);

            return Json(new { success = resultado, 
                message = resultado ? "Senha alterada com sucesso." : "Erro ao alterar senha." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirUsuario(string tipo, int userId)
        {
            var resultado = await _adminRepository.DeleteUserAsync(tipo, userId);

            return Json(new { success = resultado, 
                message = resultado ? "Usuário excluído com sucesso." : "Erro ao excluir usuário." });
        }

        #endregion

        #region Gerenciamento de Serviços

        public async Task<IActionResult> GerenciarServicos()
        {
            var servicos = await _servicoRepository.GetAllServicosAsync();
            var funcionarios = await _funcionarioRepository.GetAllFuncionariosAsync();

            var viewModel = new ServicoManagementViewModel
            {
                Servicos = servicos.Select(s => new ServicoListViewModel
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    Descricao = s.Descricao,
                    QuantidadeFuncionarios = s.Funcionarios.Count,
                    NomesFuncionarios = s.Funcionarios.Select(f => f.Nome).ToList()
                }).ToList(),
                NovoServico = new ServicoViewModel
                {
                    Funcionarios = funcionarios.Select(f => new SelectListItem
                    {
                        Value = f.Id.ToString(),
                        Text = f.Nome
                    })
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarServico(ServicoViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _servicoRepository.ServicoExistsAsync(model.Nome))
                {
                    ModelState.AddModelError("Nome", "Já existe um serviço com este nome.");
                    return RedirectToAction(nameof(GerenciarServicos));
                }

                var servico = new Servico
                {
                    Nome = model.Nome,
                    Descricao = model.Descricao
                };

                if (await _servicoRepository.AddAsync(servico))
                {
                    if (model.FuncionarioIds != null && model.FuncionarioIds.Any())
                    {
                        foreach (var funcionarioId in model.FuncionarioIds)
                        {
                            await _servicoRepository.AssignFuncionarioToServicoAsync(servico.Id, funcionarioId);
                        }
                    }

                    TempData["StatusMessage"] = "Serviço adicionado com sucesso.";
                    return RedirectToAction(nameof(GerenciarServicos));
                }
            }

            TempData["ErrorMessage"] = "Erro ao adicionar serviço.";
            return RedirectToAction(nameof(GerenciarServicos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarServico(ServicoViewModel model)
        {
            if (ModelState.IsValid)
            {
                var servico = await _servicoRepository.GetServicoByIdAsync(model.Id);
                if (servico == null)
                    return NotFound();

                servico.Nome = model.Nome;
                servico.Descricao = model.Descricao;

                if (await _servicoRepository.UpdateAsync(servico))
                {
                    // Atualiza os funcionários associados
                    var funcionariosAtuais = servico.Funcionarios.Select(f => f.Id).ToList();
                    var funcionariosNovos = model.FuncionarioIds ?? new List<int>();

                    // Remove funcionários que não estão mais associados
                    foreach (var funcionarioId in funcionariosAtuais.Except(funcionariosNovos))
                    {
                        await _servicoRepository.RemoveFuncionarioFromServicoAsync(servico.Id, funcionarioId);
                    }

                    // Adiciona novos funcionários
                    foreach (var funcionarioId in funcionariosNovos.Except(funcionariosAtuais))
                    {
                        await _servicoRepository.AssignFuncionarioToServicoAsync(servico.Id, funcionarioId);
                    }

                    TempData["StatusMessage"] = "Serviço atualizado com sucesso.";
                    return RedirectToAction(nameof(GerenciarServicos));
                }
            }

            TempData["ErrorMessage"] = "Erro ao atualizar serviço.";
            return RedirectToAction(nameof(GerenciarServicos));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirServico(int id)
        {
            if (await _servicoRepository.DeleteAsync(id))
            {
                TempData["StatusMessage"] = "Serviço excluído com sucesso.";
            }
            else
            {
                TempData["ErrorMessage"] = "Erro ao excluir serviço.";
            }

            return RedirectToAction(nameof(GerenciarServicos));
        }

        [HttpGet]
        public async Task<IActionResult> GetServicoDetails(int id)
        {
            var servico = await _servicoRepository.GetServicoByIdAsync(id);
            if (servico == null)
                return NotFound();

            var viewModel = new ServicoViewModel
            {
                Id = servico.Id,
                Nome = servico.Nome,
                Descricao = servico.Descricao,
                FuncionarioIds = servico.Funcionarios.Select(f => f.Id).ToList()
            };

            return Json(viewModel);
        }

        #endregion
    }
}
