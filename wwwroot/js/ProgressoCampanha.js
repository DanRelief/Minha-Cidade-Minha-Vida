let graficoProgresso;

function renderizarGrafico(porcentagem) {
    const canvas = document.getElementById('graficoCampanha');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    document.getElementById('total-porcentagem').innerText = porcentagem + "%";

    // Destruir gráfico anterior se existir para evitar sobreposição
    if (graficoProgresso) {
        graficoProgresso.destroy();
    }

    graficoProgresso = new Chart(ctx, {
        type: 'doughnut',
        data: {
            datasets: [{
                data: [porcentagem, 100 - porcentagem],
                backgroundColor: ['#18b07c', '#f3b5b5'], // Verde Concluído / Rosa Pendente
                borderWidth: 0,
                hoverOffset: 0
            }]
        },
        options: {
            cutout: '80%', // Deixa o anel mais fino para o número caber no meio
            responsive: true,
            maintainAspectRatio: false, // CRÍTICO: faz o gráfico respeitar o tamanho do container CSS
            plugins: {
                tooltip: { enabled: false },
                legend: { display: false }
            }
        }
    });
}

// Função para garantir que o gráfico carregue ao mudar o select E ao abrir a página
document.addEventListener('DOMContentLoaded', function () {
    const btnTeste = document.getElementById('btnTestarGrafico');

    btnTeste?.addEventListener('click', function () {
        // 1. Dados de exemplo para simular conclusão
        const dadosSimulados = [
            { nome: "Alimentos", porcentagem: 100, atual: 50, meta: 50, unidade: "KG" },
            { nome: "Roupas", porcentagem: 100, atual: 20, meta: 20, unidade: "UNI" }
        ];

        // 2. Tenta chamar as funções do seu arquivo ProgressoCampanha.js
        if (typeof renderizarGrafico === "function") {
            renderizarGrafico(100);

            // Simula a criação manual das barras caso renderizarCategorias não exista
            const lista = document.getElementById('listaCategorias');
            if (lista) {
                lista.innerHTML = ""; // Limpa atual
                dadosSimulados.forEach(cat => {
                    lista.innerHTML += `
                        <div class="categoria-item">
                            <div class="categoria-info">
                                <span>${cat.nome}</span>
                                <span>${cat.porcentagem}%</span>
                            </div>
                            <div class="barra-progresso">
                                <div class="barra-preenchimento" style="width: 100%; background-color: #18b07c;"></div>
                            </div>
                            <p class="meta-texto">${cat.atual}/${cat.meta} ${cat.unidade}</p>
                        </div>
                    `;
                });
            }
            console.log("Simulação de 100% aplicada com sucesso.");
        } else {
            console.error("Função renderizarGrafico não encontrada. Verifique se o ProgressoCampanha.js foi carregado corretamente.");
        }
    });
});