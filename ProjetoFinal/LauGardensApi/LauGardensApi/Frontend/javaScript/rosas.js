// --- rosas.js ---

// 1. Configuração da API (Confirma se a porta 7010 é a correta do teu Swagger)
const API_BASE_URL = "https://localhost:7010";

// 2. Função para formatar preços (Ex: 15.00 -> 15,00 €)
function formatarMoeda(valor) {
    if (valor === undefined || valor === null) return 'Sob Consulta';
    return valor.toLocaleString('pt-PT', { style: 'currency', currency: 'EUR' });
}

// 3. Função Principal: Carregar Rosas
async function carregarPlantas() {
    const container = document.getElementById("plantas-container");
    const modaisContainer = document.getElementById("modais-container");

    try {
        // Faz o pedido ao teu PlantasController
        const resposta = await fetch(`${API_BASE_URL}/api/Plantas`);

        if (!resposta.ok) throw new Error("Erro ao ligar à API");

        const plantas = await resposta.json();

        // Filtra apenas as Rosas (Opcional: se a API trouxer tudo misturado)
        // Se a tua API já traz só rosas, podes remover o .filter
        const rosas = plantas.filter(p => p.categoria && p.categoria.toLowerCase().includes('rosa'));

        // Se a lista vier vazia ou o filtro remover tudo, usa 'plantas' diretamente se preferires
        const listaFinal = rosas.length > 0 ? rosas : plantas;

        let cartoesHtml = '';
        let modaisHtml = '';

        listaFinal.forEach(planta => {
            const modalId = `modalPlanta${planta.id}`;

            // A. HTML DO CARTÃO (O quadrado pequeno)
            cartoesHtml += `
            <div class="col-sm-6 col-md-4 justify-content-center">
              <div class="card h-100 border-0 shadow-sm" data-aos="fade-in">
                <img src="${planta.urlImagem}" class="card-img-top" alt="${planta.nome}" 
                     style="height: 250px; object-fit: cover;"
                     onerror="this.onerror=null; this.src='/Frontend/img/default-flower.png';">
                
                <div class="card-body text-center">
                  <h5 class="card-title fw-bold">${planta.nome}</h5>
                  <p class="text-muted mb-2">${formatarMoeda(planta.preco)}</p>
                  
                  <a href="#" class="btn btn-sm btn-outline-success rounded-pill px-4" 
                     data-bs-toggle="modal" data-bs-target="#${modalId}">
                    Ver Detalhes
                  </a>
                </div>
              </div>
            </div>`;

            // B. HTML DO MODAL (A janela que abre)
            modaisHtml += `
            <div class="modal fade" id="${modalId}" tabindex="-1" aria-hidden="true">
              <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                  <div class="modal-header border-0">
                    <h5 class="modal-title fw-bold">• ${planta.nome}</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                  </div>
                  
                  <div class="modal-body text-center">
                    <img src="${planta.urlImagem}" class="img-fluid rounded mb-3" 
                         style="max-height: 300px;"
                         onerror="this.onerror=null; this.src='/Frontend/img/default-flower.png';">
                    
                    <p class="text-start px-3">${planta.descricao || 'Sem descrição disponível.'}</p>
                    
                    <div class="bg-light p-3 rounded text-start mx-3">
                        <small><strong>Categoria:</strong> ${planta.categoria}</small><br>
                        <small><strong>Preço:</strong> ${formatarMoeda(planta.preco)}</small>
                    </div>
                  </div>

                  <div class="modal-footer justify-content-center border-0 mb-3">
                    <button class="btn btn-success w-75" onclick="adicionarAoCarrinho(${planta.id})">
                      Adicionar ao Carrinho 🛒
                    </button>
                  </div>
                </div>
              </div>
            </div>`;
        });

        // Injeta o HTML na página
        container.innerHTML = cartoesHtml;
        modaisContainer.innerHTML = modaisHtml;

    } catch (erro) {
        console.error(erro);
        container.innerHTML = `<p class="text-center text-danger w-100">⚠️ Não foi possível carregar as rosas. (Verifica se a API está ligada)</p>`;
    }
}

// 4. Função Dummy para o botão não dar erro
function adicionarAoCarrinho(id) {
    alert(`Planta ${id} adicionada ao carrinho! (Funcionalidade em desenvolvimento)`);
}

// 5. Iniciar assim que a página carrega
document.addEventListener('DOMContentLoaded', carregarPlantas);