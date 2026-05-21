using MCMV.Data;
using MCMV.Logical;
using MCMV.Models;
using MySql.Data.MySqlClient;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mysqlx.Expr;
using System.Text.RegularExpressions;
using static MCMV.Models.CampanhaModel;
using System.Collections.Generic;

namespace MCMV.Controllers
{
    public class HomeController : Controller
    {
        private readonly LocalizacaoService _localizacaoService;
        private readonly LoginService _loginService;
        private readonly RegisterService _registerService;
        private readonly DonationService _donationService;
        private readonly AlteracoesService _alteracoesService;

        public HomeController(LoginService loginService, RegisterService registerService, DonationService donationService, AlteracoesService alteracoesService, LocalizacaoService mapaService)
        {
            _loginService = loginService;
            _registerService = registerService;
            _donationService = donationService;
            _alteracoesService = alteracoesService;
            _localizacaoService = mapaService;


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

                HttpContext.Session.SetString("Documento", docLimpo);

                return tipo == "CPF" ? RedirectToAction("IndexUsuario") : RedirectToAction("IndexInstituicao");
            }

            ViewBag.Erro = "CPF/CNPJ ou senha inválidos";
            return View();
        }

        // --- CADASTRO ---

        [HttpGet]
        public IActionResult Cadastro() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Cadastro(string user, string identific, bool isInstit, string email, string senha, string confirmarSenha, IFormFile documentoInstituicao)
        {
            // 1.Remove tudo que não for número
            string docLimpo = new string(identific.Where(char.IsDigit).ToArray());

            // 2. Validação simplificada de tamanho (CPF=11, CNPJ=14)
            if (ValidadoresService.ValidarDocumento(docLimpo) == false)
            {
                ViewBag.Erro = "O documento deve ter 11 dígitos (CPF) ou 14 dígitos (CNPJ).";
                return View("Cadastro");
            }

            bool instituicaoVerificada = await ValidadoresService.VerificarInstituicao(docLimpo);

            // 3. Verificar duplicidade e salvar
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



        //----Instituição----
        [HttpGet]
        public IActionResult IndexInstituicao()
        {
            // Pegamos o documento da sessão
            string? instDoc = HttpContext.Session.GetString("Documento");

            if (string.IsNullOrEmpty(instDoc))
                return RedirectToAction("Login");

            // Buscamos as campanhas no banco
            var campanhas = _registerService.ListarCampanhasPorInstituicao(instDoc);

            // Enviamos a lista para a View
            return View(campanhas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FazerVerificacao(IFormFile imagem)
        {
            //string cnpjlimpo = new string((cnpj ?? "").Where(char.IsDigit).ToArray());
            string urlModal = Url.Action(nameof(IndexInstituicao), "Home") + "#modal-verificacao";
            var docSessao = HttpContext.Session.GetString("Documento");

            if(string.IsNullOrWhiteSpace(docSessao))
                return RedirectToAction("Login");


            string cnpj = new string(docSessao.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14)
            {
                TempData["VerifTipo"] = "erro";
                TempData["VerifMensagem"] = "Apenas instituições (CNPJ) podem se verificar.";
                return Redirect(urlModal);
            }


            //2) verifica se existe e se já está verificada
            bool? jaVerificada = _alteracoesService.JaEstaVerificada(cnpj);

            if (jaVerificada == null)
            {
                TempData["VerifTipo"] = "erro";
                TempData["VerifMensagem"] = "CNPJ não encontrado/cadastrado.";
                return Redirect(urlModal);
            }

            if (jaVerificada == true)
            {
                TempData["VerifTipo"] = "aviso"; 
                TempData["VerifMensagem"] = "Esta instituição já está verificada ✅";
                return Redirect(urlModal);
            }

            if (imagem == null || imagem.Length == 0)
            {
                TempData["VerifTipo"] = "erro";
                TempData["VerifMensagem"] = "Imagem não enviada";
                return Redirect(urlModal);
            }

            bool ok = _alteracoesService.mudarValidacaoInstituicao(cnpj);

            if (!ok)
            {
                TempData["VerifTipo"] = "erro";
                TempData["VerifMensagem"] = "Não foi possível atualizar. Verifique se o CNPJ está cadastrado.";
                return Redirect(urlModal);
            }


            TempData["VerifTipo"] = ok ? "sucesso" : "erro";
            TempData["VerifMensagem"] = ok ? "Verificação realizada com sucesso!" : "Não foi possível atualizar a verificação.";

            return Redirect(urlModal);
        }


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
        public IActionResult FacaUmaDoacao()
        {
            var instituicoes = _registerService.ListarInstituicoes() as List<MCMV.Models.UserViewModel>
                               ?? _registerService.ListarInstituicoes();

            ViewBag.Instituicoes = instituicoes;

            return View();
        }

        [HttpPost]
        public IActionResult EnviarDoacao(FazerUmaDoacao doacao)
        {
            if (ModelState.IsValid)
            {
                string documentoLogado = HttpContext.Session.GetString("Documento") ?? "000000";
                _donationService.SalvarOfertaDoacao(doacao, documentoLogado);

                TempData["MensagemSucesso"] = "Oferta de doação enviada com sucesso!";
                return RedirectToAction("IndexUsuario");
            }

            return View("FacaUmaDoacao", doacao);
        }

        [HttpPost]
        public IActionResult RegistrarCampanha(
            string nomeDaCampanha, string rua, string cep, string numeroEnderec, string bairro,
            string cat1_nome, int cat1_meta, string cat1_unidade,
            string? cat2_nome, int? cat2_meta, string? cat2_unidade,
            string? cat3_nome, int? cat3_meta, string? cat3_unidade,
            DateTime data_inicio, DateTime data_fim, string? descricao)
        {
            // Pegar o documento da instituição logada (assumindo que salvou no Session no Login)
            string? instDoc = HttpContext.Session.GetString("Documento");

            if (string.IsNullOrEmpty(instDoc)) return RedirectToAction("Login");

            // Criar objeto Campanha
            var novaCamp = new CampanhaModel
            {
                Nome = nomeDaCampanha,
                Rua = rua,
                Cep = cep,
                Numero = numeroEnderec,
                Bairro = bairro,
                DataInicio = data_inicio,
                DataFim = data_fim,
                Descricao = descricao,
                DocumentoInstituicao = instDoc
            };

            var listaCategorias = new List<CategoriaCampanhaModel>();
            listaCategorias.Add(new CategoriaCampanhaModel { Nome = cat1_nome, Meta = cat1_meta, Unidade = cat1_unidade });

            if (!string.IsNullOrEmpty(cat2_nome))
                listaCategorias.Add(new CategoriaCampanhaModel { Nome = cat2_nome, Meta = cat2_meta ?? 0, Unidade = cat2_unidade ?? "UNI" });

            if (!string.IsNullOrEmpty(cat3_nome))
                listaCategorias.Add(new CategoriaCampanhaModel { Nome = cat3_nome, Meta = cat3_meta ?? 0, Unidade = cat3_unidade ?? "UNI" });

            // Salvar
            _registerService.RegistrarCampanha(novaCamp, listaCategorias);

            TempData["VerifMensagem"] = "Campanha registrada com sucesso!";
            TempData["VerifTipo"] = "sucesso";

            return RedirectToAction("IndexInstituicao");
        }

        [HttpGet]
        public IActionResult ObterProgressoCampanha(int id)
        {
            // 1. Busca as categorias vinculadas a essa campanha no banco
            var categorias = _donationService.ListarCategoriasPorCampanha(id);

            if (categorias == null || !categorias.Any()) return NotFound();

            // 2. Calcula o total geral para o gráfico em anel
            double metaTotal = categorias.Sum(c => c.Meta);
            double atualTotal = categorias.Sum(c => c.Atual);
            double porcentagemGeral = metaTotal > 0 ? (atualTotal / metaTotal) * 100 : 0;

            return Json(new
            {
                porcentagemGeral = Math.Round(porcentagemGeral, 1),
                categorias = categorias.Select(c => new {
                    nome = c.Nome,
                    meta = c.Meta,
                    atual = c.Atual,
                    unidade = c.Unidade,
                    porcentagem = c.Meta > 0 ? Math.Round(((double)c.Atual / c.Meta) * 100, 1) : 0
                })
            });
        }

        

        //Aba Meus Dados
        public IActionResult MeusDados()
        {
            // 1. Buscamos o documento usando a chave correta: "Documento"
            string documentoLogado = HttpContext.Session.GetString("Documento");

            // 2. Se a sessão expirou ou não gravou, ele volta pro login
            if (string.IsNullOrEmpty(documentoLogado))
            {
                return RedirectToAction("Login");
            }

            var resumo = _donationService.ObterResumoUsuario(documentoLogado);

            // 3. Retornamos a View passando esse objeto
            return View(resumo);
        }

        //Aba Portal Transparência
        public IActionResult PortalTransparencia()
        {
            var dados = _donationService.ObterDadosPortal();
            return View(dados); 
        }

        //Controller de Mapa
        [HttpGet]
        public async Task<IActionResult> MapaCampanhas()
        {
            var dados = await _localizacaoService.ObterCampanhasNoMapaAsync();
            return Json(dados);
        }

        //Saindo da Sessão

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

    }
}