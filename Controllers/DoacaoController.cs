using Microsoft.AspNetCore.Mvc;
using MCMV.Models;
using MCMV.Logical; // Namespace onde está o seu DonationService

namespace MCMV.Controllers
{
    // 1. Adicionada a herança de Controller
    public class DoacaoController : Controller
    {
        private readonly DonationService _donationService;

        // 2. Construtor para receber o serviço de banco de dados
        public DoacaoController(DonationService donationService)
        {
            _donationService = donationService;
        }

        // 3. Removido o 'static' - agora o método reconhece o comando View()
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
                try
                {
                    // 4. Usa o serviço para salvar no MySQL
                    _donationService.SalvarSolicitacao(solicitacao);

                    TempData["MensagemSucesso"] = "Solicitação enviada com sucesso!";

                    // Redireciona para a Home do Usuário (ajuste o nome se necessário)
                    return RedirectToAction("IndexUsuario", "Home");
                }
                catch (System.Exception ex)
                {
                    ViewBag.Erro = "Erro ao salvar no banco: " + ex.Message;
                }
            }

            // Se algo falhar, volta para a tela de formulário
            return View("PrecisaDeDoacao", solicitacao);
        }
    }
}