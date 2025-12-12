// URL da API
const API_URL = "https://localhost:7010/api/Plantas";

async function carregarPlantas() {
    const container = document.getElementById("plantas-container");

    // Se não houver container, não fazemos nada
    if (!container) return;

    // Garante que o modal genérico existe no DOM
    const modalId = "modalPlantaGenerico";
    if (!document.getElementById(modalId)) {
        criarModalGenerico(modalId);
    }

    const categoriaAlvo = container.getAttribute("data-categoria");
    if (!categoriaAlvo) {
        console.warn("Container #plantas-container sem atributo data-categoria.");
        return;
    }

    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Erro ao carregar plantas da API");

        const todasPlantas = await response.json();
        const plantasFiltradas = todasPlantas.filter(p => {
            if (!p.categoria) return false;
            return p.categoria.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase() === categoriaAlvo.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
        });

        container.innerHTML = "";

        if (plantasFiltradas.length === 0) {
            // Retrieve unique categories for debugging
            const categoriasDisponiveis = [...new Set(todasPlantas.map(p => p.categoria))].join(", ");
            const debugInfo = categoriasDisponiveis ? `Categorias encontradas na API: ${categoriasDisponiveis}` : "Nenhuma categoria encontrada na API.";

            container.innerHTML = `
                <div class='col-12 text-center mt-3'>
                    <p class='text-warning'>Não foram encontradas plantas na categoria "${categoriaAlvo}".</p>
                    <p class='text-muted small'>${debugInfo}</p>
                </div>`;
            return;
        }

        plantasFiltradas.forEach(p => {
            const col = document.createElement("div");
            col.className = "col-sm-6 col-md-4 justify-content-center";

            let imagemUrl = processarImagemUrl(p.urlImagem);

            // Escape special chars in JSON for the onclick handler
            const plantaJson = JSON.stringify(p).replace(/'/g, "&#39;").replace(/"/g, "&quot;");

            col.innerHTML = `
                <div class="card h-100">
                    <img src="${imagemUrl}" class="card-img-top" alt="${p.nome}">
                    <div class="card-body text-center">
                        <h5 class="card-title">${p.nome}</h5>
                        <!-- <p class="card-text text-muted">${p.descricao ? p.descricao.substring(0, 50) + '...' : ''}</p> -->
                        <a class="cardInfo" style="cursor: pointer;" onclick='abrirModal(${plantaJson})'>Ver mais</a>
                    </div>
                </div>
            `;
            container.appendChild(col);
        });

    } catch (err) {
        console.error(err);
        container.innerHTML = "<p class='text-center mt-3 text-danger'>Erro ao carregar as plantas.</p>";
    }
}

function processarImagemUrl(url) {
    if (!url) return "";
    if (url.startsWith("/img/")) {
        return "../.." + url;
    } else if (!url.startsWith("/")) {
        return "../../" + url;
    }
    return url;
}

function criarModalGenerico(id) {
    const modalHtml = `
    <div class="modal fade" id="${id}" tabindex="-1" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="modalTitulo"></h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Fechar"></button>
          </div>
          <div class="modal-body text-center">
            <img id="modalImagem" src="" alt="" style="max-width: 75%; display: block; margin: 0 auto 15px;">
            <p id="modalDescricao" class="text-start"></p>
            
            <div class="d-flex justify-content-between align-items-center mb-3">
                <span id="modalPreco" class="fw-bold fs-5"></span>
                <span id="modalStock" class="badge bg-secondary"></span>
            </div>

            <hr>
            
            <div class="d-grid gap-2">
                <button id="btnAdicionarCarrinho" type="button" class="btn p-2"  style="background-color: rgb(47, 70, 58); color: white;">
                    Adicionar ao Carrinho <i class="bi bi-cart-plus"></i>
                </button>
            </div>
            <div id="carrinhoMensagem" class="mt-2"></div>

          </div>
        </div>
      </div>
    </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHtml);
}



// Global function called by onclick
window.abrirModal = function (planta) {
    const modalEl = document.getElementById("modalPlantaGenerico");
    const modalTitulo = document.getElementById("modalTitulo");
    const modalImagem = document.getElementById("modalImagem");
    const modalDescricao = document.getElementById("modalDescricao");
    const modalPreco = document.getElementById("modalPreco");
    const modalStock = document.getElementById("modalStock");
    const btnCarrinho = document.getElementById("btnAdicionarCarrinho");
    const carrinhoMensagem = document.getElementById("carrinhoMensagem");

    modalTitulo.innerText = "• " + planta.nome;
    modalImagem.src = processarImagemUrl(planta.urlImagem);
    modalImagem.alt = planta.nome;

    // Fallback if no detailed description
    modalDescricao.innerText = planta.descricao || "Sem descrição disponível.";

    if (planta.preco) {
        modalPreco.innerText = `${Number(planta.preco).toFixed(2)} €`;
    } else {
        modalPreco.innerText = "";
    }

    if (planta.stock && planta.stock.quantidade > 0) {
        modalStock.innerText = `Stock: ${planta.stock.quantidade}`;
        btnCarrinho.disabled = false;
    } else {
        modalStock.innerText = "Sem Stock";
        btnCarrinho.disabled = true;
    }

    carrinhoMensagem.innerHTML = ""; // Clear previous messages

    // Update Button Click Logic
    btnCarrinho.onclick = function () {
        adicionarAoCarrinho(planta);
    };

    const modal = new bootstrap.Modal(modalEl);
    modal.show();
};

function adicionarAoCarrinho(planta) {
    let carrinho = JSON.parse(localStorage.getItem('lausGardenCart')) || [];
    const itemExistente = carrinho.find(item => item.id === planta.id);

    if (itemExistente) {
        itemExistente.quantidade += 1;
    } else {
        carrinho.push({
            id: planta.id,
            nome: planta.nome,
            preco: planta.preco,
            imagem: planta.urlImagem,
            quantidade: 1
        });
    }

    localStorage.setItem('lausGardenCart', JSON.stringify(carrinho));

    const msgDiv = document.getElementById("carrinhoMensagem");
    msgDiv.innerHTML = "<span class='text-success fw-bold'>Adicionado ao carrinho!</span>";

    // Optional timeout to clear message
    setTimeout(() => {
        msgDiv.innerHTML = "";
    }, 2000);
}

document.addEventListener("DOMContentLoaded", carregarPlantas);
