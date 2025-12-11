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
        const plantasFiltradas = todasPlantas.filter(p => p.categoria === categoriaAlvo);

        container.innerHTML = "";

        if (plantasFiltradas.length === 0) {
            container.innerHTML = `<p class='text-center mt-3'>Não foram encontradas plantas na categoria "${categoriaAlvo}".</p>`;
            return;
        }

        plantasFiltradas.forEach(p => {
            const col = document.createElement("div");
            col.className = "col-sm-6 col-md-4 justify-content-center";

            let imagemUrl = processarImagemUrl(p.urlImagem);

            // Escape special chars in JSON for the onclick handler
            const plantaJson = JSON.stringify(p).replace(/'/g, "&#39;").replace(/"/g, "&quot;");

            col.innerHTML = `
                <div class="card h-100" data-aos="fade-in" data-aos-duration="1000">
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
            <p id="modalPreco" class="fw-bold"></p>

            <hr>
            <h6 class="text-start">Reservar Planta</h6>
            <form id="reservaForm" class="text-start">
                <input type="hidden" id="reservaPlantaId">
                <div class="mb-2">
                    <label for="reservaNome" class="form-label">Nome:</label>
                    <input type="text" class="form-control" id="reservaNome" required>
                </div>
                <div class="mb-2">
                    <label for="reservaContacto" class="form-label">Contacto (Email/Tel):</label>
                    <input type="text" class="form-control" id="reservaContacto" required>
                </div>
                <div class="d-grid mt-3">
                    <button type="submit" class="btn btn-success">Confirmar Reserva</button>
                </div>
            </form>
            <div id="reservaMensagem" class="mt-2"></div>

          </div>
        </div>
      </div>
    </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHtml);

    // Attach generic submit handler once
    document.addEventListener("submit", async function (e) {
        if (e.target && e.target.id === "reservaForm") {
            e.preventDefault();
            const plantaId = document.getElementById("reservaPlantaId").value;
            const nome = document.getElementById("reservaNome").value;
            const contacto = document.getElementById("reservaContacto").value;
            const msgDiv = document.getElementById("reservaMensagem");

            msgDiv.innerHTML = "<span class='text-primary'>A processar...</span>";

            try {
                const response = await fetch("/api/Reservas", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        NomeCliente: nome,
                        Contacto: contacto,
                        PlantaId: parseInt(plantaId)
                    })
                });

                if (response.ok) {
                    msgDiv.innerHTML = "<span class='text-success'>Reserva efetuada com sucesso! Entre em contacto para levantamento.</span>";
                    e.target.reset();
                    setTimeout(() => {
                        const modalEl = document.getElementById("modalPlantaGenerico");
                        const modal = bootstrap.Modal.getInstance(modalEl);
                        modal.hide();
                        msgDiv.innerHTML = "";
                    }, 3000);
                } else {
                    const errText = await response.text();
                    msgDiv.innerHTML = `<span class='text-danger'>Erro: ${errText}</span>`;
                }
            } catch (err) {
                console.error(err);
                msgDiv.innerHTML = "<span class='text-danger'>Erro de comunicação com o servidor.</span>";
            }
        }
    });
}


// Global function called by onclick
window.abrirModal = function (planta) {
    const modalEl = document.getElementById("modalPlantaGenerico");
    const modalTitulo = document.getElementById("modalTitulo");
    const modalImagem = document.getElementById("modalImagem");
    const modalDescricao = document.getElementById("modalDescricao");
    const modalPreco = document.getElementById("modalPreco");
    const reservaPlantaId = document.getElementById("reservaPlantaId");
    const reservaMensagem = document.getElementById("reservaMensagem");

    modalTitulo.innerText = "• " + planta.nome;
    modalImagem.src = processarImagemUrl(planta.urlImagem);
    modalImagem.alt = planta.nome;

    // Fallback if no detailed description
    modalDescricao.innerText = planta.descricao || "Sem descrição disponível.";

    if (planta.preco) {
        modalPreco.innerText = `Preço: ${Number(planta.preco).toFixed(2)} €`;
    } else {
        modalPreco.innerText = "";
    }

    // Set hidden ID for reservation
    reservaPlantaId.value = planta.id;
    reservaMensagem.innerHTML = ""; // Clear previous messages

    const modal = new bootstrap.Modal(modalEl);
    modal.show();
};

document.addEventListener("DOMContentLoaded", carregarPlantas);
