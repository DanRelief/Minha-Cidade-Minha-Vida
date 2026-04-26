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

        // Login
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string documento, string senha)
        {
            bool valido = _loginService.ValidarLogin(documento, senha);

            if (valido)
            {
                string tipo = _loginService.ObterTipoUsuario(documento);

                if (tipo == "CPF")
                {
                    return RedirectToAction("IndexUsuario");
                }
                else if (tipo == "CNPJ")
                {
                    return RedirectToAction("IndexInstituicao");
                }
            }

            ViewBag.Erro = "CPF/CNPJ ou senha inválidos";
            return View();
        }

        //Cadastro
        [HttpGet]
        public IActionResult Cadastro() => View();

    // Para inserir os dados recebidos do cadastro 
    [HttpPost]
    public async Task<IActionResult> Cadastro(string user, string identific, bool isInstit, string email, string senha, string confirmarSenha, IFormFile documentoInstituicao)
    {
        // Validação de E-mail
        string padraoEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (!Regex.IsMatch(email ?? "", padraoEmail))
        {
            ViewBag.Erro = "E-mail inválido!";
            return View("Cadastro");
        }

        // Validação de Senha
        if (senha != confirmarSenha)
        {
            ViewBag.Erro = "As senhas não coincidem!";
            return View("Cadastro");
        }

        // Validação de CPF ou CNPJ
        if (!Validador.ValidarDocumento(identific))
        {
            ViewBag.Erro = "CPF ou CNPJ inválido!";
            return View("Cadastro");
        }



        bool instituicaoVerificada = false;


        if (isInstit && identific.Length == 14)
        {
            // Tentativa 1: API
            try
            {
                instituicaoVerificada =
                    await Validador_Instituicao.VerificarInstituicao(identific);
            }
            catch
            {
                // API falhou → ignora
                instituicaoVerificada = false;
            }

            // Tentativa 2: Documento enviado
            if (!instituicaoVerificada &&
                documentoInstituicao != null &&
                documentoInstituicao.Length > 0)
            {
                instituicaoVerificada = true;
            }
        }





        // Verifica se o documento de usuário escolhido já existe no banco de dados para evitar duplicidade.
        if (_registerService.UsuarioExiste(identific))
        {
            ViewBag.Erro = "Este usuário já está em uso.";
            return View("Cadastro");
        }

        // Se todas as validações passaram, cria o novo usuário no banco de dados.
        _registerService.CriarUsuario(user, senha, email, identific, instituicaoVerificada);

        if (isInstit)
        {
            if (instituicaoVerificada)
            {
                ViewBag.Mensagem =
                    "Analisamos seu CNPJ e o Documento Enviado e você é uma Instituição Verificada de Confiança!";
                ViewBag.Tipo = "sucesso";
            }
            else
            {
                ViewBag.Mensagem =
                    "Analisamos seu CNPJ e não encontramos a sua Instituição no Mapa OSC para Verificação de Conta.";
                ViewBag.Tipo = "aviso";
            }

            return View("Login");
        }

        //  mensagem de sucesso para o usuário após o cadastro, informando que ele pode fazer login.            
        TempData["MensagemSucesso"] = "Usuário criado com sucesso! Faça login para continuar.";

            TempData["MensagemSucesso"] = "Usuário criado com sucesso!";
            return RedirectToAction("Login");
        }

        //tela inicial
        public IActionResult IndexUsuario() => View();

        public IActionResult IndexInstituicao() => View();


        [HttpGet]
        public IActionResult PrecisaDeDoacao()
        {
            return View();
        }

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
        public IActionResult FacaUmaDoacao()
        {
            return View();
        }

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