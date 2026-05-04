using Microsoft.AspNetCore.Mvc;
using MCMV.Data;
using MCMV.Logical;
using MCMV.Models;
using System.Text.RegularExpressions;

namespace MCMV.Controllers
{
    public class HomeController : Controller
    {
        private readonly LoginService _loginService;
        private readonly RegisterService _registerService;
        private readonly DonationService _donationService;

        public HomeController(LoginService loginService, RegisterService registerService, DonationService donationService)
        {
            _loginService = loginService;
            _registerService = registerService;
            _donationService = donationService;
        }

        // --- LOGIN ---
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string documento, string senha)
        {
            if (string.IsNullOrEmpty(documento))
            {
                ViewBag.Erro = "Por favor, preencha o campo de documento.";
                return View();
            }

            string docLimpo = new string(documento.Where(char.IsDigit).ToArray());

            bool valido = _loginService.ValidarLogin(docLimpo, senha);

            if (valido)
            {
                string tipo = _loginService.ObterTipoUsuario(docLimpo);
                return tipo == "CPF" ? RedirectToAction("IndexUsuario") : RedirectToAction("IndexInstituicao");
            }

            ViewBag.Erro = "CPF/CNPJ ou senha inválidos";
            return View();
        }

        // --- CADASTRO ---
        [HttpGet]
        public IActionResult Cadastro() => View();
        public async Task<IActionResult> Cadastro(string user, string identific, bool isInstit, string email, string senha, string confirmarSenha, IFormFile documentoInstituicao)
        {
            // 1. Limpeza radical: remove tudo que não for número
            string docLimpo = new string(identific.Where(char.IsDigit).ToArray());

            // 2. Validação simplificada de tamanho (CPF=11, CNPJ=14)
            if (docLimpo.Length != 11 && docLimpo.Length != 14)
            {
                ViewBag.Erro = "O documento deve ter 11 dígitos (CPF) ou 14 dígitos (CNPJ).";
                return View("Cadastro");
            }

            // 3. Forçar sempre como True (conforme solicitado)
            bool instituicaoVerificada = true;

            // 4. Verificar duplicidade e salvar
            if (_registerService.UsuarioExiste(docLimpo))
            {
                ViewBag.Erro = "Este CPF ou CNPJ já está cadastrado.";
                return View("Cadastro");
            }

            _registerService.CriarUsuario(user, senha, email, docLimpo, instituicaoVerificada);

            TempData["MensagemSucesso"] = "Cadastro realizado com sucesso!";
            return RedirectToAction("Login");
        }

        // --- OUTRAS ROTAS ---
        public IActionResult IndexUsuario() => View();

        public IActionResult IndexInstituicao() => View();

        public IActionResult VerInstituicoes()
        {
            var instituicoes = _registerService.ListarInstituicoes();
            return View(instituicoes);
        }

        [HttpGet]
        public IActionResult PrecisaDeDoacao() => View();

        [HttpPost]
        public IActionResult EnviarSolicitacao(SolicitacaoDoacao solicitacao)
        {
            if (ModelState.IsValid)
            {
                _donationService.SalvarSolicitacao(solicitacao);
                TempData["MensagemSucesso"] = "Solicitação enviada com sucesso!";
                return RedirectToAction("IndexUsuario");
            }
            return View("PrecisaDeDoacao", solicitacao);
        }

        [HttpGet]
        public IActionResult FacaUmaDoacao() => View();

        [HttpPost]
        public IActionResult EnviarDoacao(FazerUmaDoacao doacao)
        {
            if (ModelState.IsValid)
            {
                _donationService.SalvarOfertaDoacao(doacao);
                TempData["MensagemSucesso"] = "Oferta de doação enviada com sucesso!";
                return RedirectToAction("IndexUsuario");
            }
            return View("FacaUmaDoacao", doacao);
        }
    }
}