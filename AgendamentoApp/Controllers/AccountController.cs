using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using AgendamentoApp.Models;
using AgendamentoApp.Interfaces;
using AgendamentoApp.Entities;

namespace AgendamentoApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IAdminRepository _adminRepository;

        public AccountController(
            IClienteRepository clienteRepository,
            IFuncionarioRepository funcionarioRepository,
            IAdminRepository adminRepository)
        {
            _clienteRepository = clienteRepository;
            _funcionarioRepository = funcionarioRepository;
            _adminRepository = adminRepository;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Tenta autenticar como Cliente
                if (await _clienteRepository.ValidateCredentialsAsync(model.Email, model.Senha))
                {
                    var cliente = await _clienteRepository.GetClienteByEmailAsync(model.Email);
                    await SignInAsync(cliente.Id.ToString(), model.Email, "Cliente", model.RememberMe);
                    return RedirectToAction("Dashboard", "Cliente");
                }

                // Tenta autenticar como Funcionário
                if (await _funcionarioRepository.ValidateCredentialsAsync(model.Email, model.Senha))
                {
                    var funcionario = await _funcionarioRepository.GetFuncionarioByEmailAsync(model.Email);
                    await SignInAsync(funcionario.Id.ToString(), model.Email, "Funcionario", model.RememberMe);
                    return RedirectToAction("Dashboard", "Funcionario");
                }

                // Tenta autenticar como Admin
                if (await _adminRepository.ValidateCredentialsAsync(model.Email, model.Senha))
                {
                    var admin = await _adminRepository.GetAdminByEmailAsync(model.Email);
                    await SignInAsync(admin.Id.ToString(), model.Email, "Admin", model.RememberMe);
                    return RedirectToAction("Dashboard", "Admin");
                }

                ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _clienteRepository.EmailExistsAsync(model.Email))
                {
                    ModelState.AddModelError(string.Empty, "Este email já está em uso.");
                    return View(model);
                }

                var cliente = new Cliente
                {
                    Nome = model.Nome,
                    Email = model.Email,
                    Endereco = model.Endereco,
                    Senha = model.Senha // Na prática, deve-se usar hash
                };

                if (await _clienteRepository.AddAsync(cliente))
                {
                    await SignInAsync(cliente.Id.ToString(), cliente.Email, "Cliente", false);
                    return RedirectToAction("Dashboard", "Cliente");
                }

                ModelState.AddModelError(string.Empty, "Erro ao criar conta. Tente novamente.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private async Task SignInAsync(string userId, string email, string role, bool rememberMe)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Login));
        }
    }
}
