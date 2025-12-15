const API_URL = "https://localhost:7010/api/Plantas";

document.addEventListener("DOMContentLoaded", () => {
    carregarPlantas();
    setupForm();
});

async function carregarPlantas() {
    const tbody = document.getElementById("tabelaPlantasBody");

    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error("Erro ao carregar lista de plantas.");

        const plantas = await response.json();

        tbody.innerHTML = "";

        if (plantas.length === 0) {
            tbody.innerHTML = "<tr><td colspan='5' class='text-center'>Nenhuma planta encontrada.</td></tr>";
            return;
        }

        const role = sessionStorage.getItem("role");

        //Esconde "Add Planta" caso nao seja "admin"
        const formContainer = document.querySelector(".card.p-4.mb-4"); // Adjust selector based on HTML structure
        const formTitle = document.querySelector("h4.mb-3"); // "Adicionar Nova Planta" titulo

        if (role !== "admin") {
            // Esconde o formulário de criação
            const form = document.getElementById("addPlantaForm");
            if (form) {
                // Tenta esconder o card inteiro se conseguir encontrar o pai
                const card = form.closest('.card');
                if (card) card.style.display = 'none';
                else form.style.display = 'none';
            }
        }

        plantas.forEach(p => {
            const tr = document.createElement("tr");



            // btn apagar só para admin
            let deleteButtonHtml = "";
            if (role === "admin") {
                deleteButtonHtml = `
                    <button class="btn btn-sm btn-outline-danger" onclick="removerPlanta(${p.id}, '${p.nome}')">
                        <i class="bi bi-trash"></i> Apagar
                    </button>`;
            } else {
                deleteButtonHtml = `<span class="text-muted"><small>N/A</small></span>`;
            }

            tr.innerHTML = `

                <td class="fw-bold">${p.nome}</td>
                <td><span class="badge bg-secondary">${p.categoria}</span></td>
                <td>
                    <div class="input-group input-group-sm" style="width: 150px;">
                        <input type="number" class="form-control" id="stock-${p.id}" value="${p.stock ? p.stock.quantidade : 0}">
                        <button class="btn btn-outline-success" onclick="atualizarStock(${p.id})">
                            <i class="bi bi-check-lg"></i>
                        </button>
                    </div>
                </td>
                <td class="text-end">
                    ${deleteButtonHtml}
                </td>
            `;
            tbody.appendChild(tr);
        });

    } catch (err) {
        console.error(err);
        tbody.innerHTML = "<tr><td colspan='5' class='text-center text-danger'>Erro ao carregar dados.</td></tr>";
    }
}

function setupForm() {
    const form = document.getElementById("addPlantaForm");
    if (!form) return;

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        // 1. Upload da Imagem (se existir)
        const fileInput = document.getElementById("imagem");
        let urlImagemFinal = "/img/placeholder.png"; // Default

        if (fileInput.files.length > 0) {
            const formData = new FormData();
            formData.append("ficheiro", fileInput.files[0]);

            try {
                const uploadResponse = await fetch("https://localhost:7010/api/Imagens/upload", {
                    method: "POST",
                    body: formData
                });

                if (uploadResponse.ok) {
                    const data = await uploadResponse.json();
                    urlImagemFinal = data.urlImagem;
                } else {
                    alert("Erro ao carregar imagem. A usar placeholder.");
                }
            } catch (err) {
                console.error("Erro upload:", err);
                alert("Erro ao carregar imagem (Exceção). A usar placeholder.");
            }
        }

        const novaPlanta = {
            nome: document.getElementById("nome").value,
            categoria: document.getElementById("categoria").value,
            preco: parseFloat(document.getElementById("preco").value),
            quantidadeInicial: parseInt(document.getElementById("quantidadeInicial").value),
            urlImagem: urlImagemFinal,
            descricao: document.getElementById("descricao").value
        };

        try {
            const token = sessionStorage.getItem("token");
            const headers = { "Content-Type": "application/json" };
            if (token) headers["Authorization"] = `Bearer ${token}`;

            const response = await fetch(API_URL, {
                method: "POST",
                headers: headers,
                body: JSON.stringify(novaPlanta)
            });

            if (response.ok) {
                alert("Planta adicionada com sucesso!");
                form.reset();
                carregarPlantas(); //Atualiza lista
            } else {
                const errorMsg = await response.text();
                alert("Erro ao adicionar: " + errorMsg);
            }
        } catch (error) {
            console.error(error);
            alert("Erro de conexão.");
        }
    });
}

window.removerPlanta = async function (id, nome) {
    if (!confirm(`Deseja eliminar a planta "${nome}"?`)) {
        return;
    }

    try {
        const token = sessionStorage.getItem("token");
        const headers = {};
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`${API_URL}/${id}`, {
            method: "DELETE",
            headers: headers
        });

        if (response.ok || response.status === 204) {
            alert("Planta eliminada com sucesso.");
            carregarPlantas(); //Atualiza lista
        } else {
            alert("Erro ao eliminar planta.");
        }
    } catch (err) {
        console.error(err);
        alert("Erro de conexão ao eliminar.");
    }
};

window.atualizarStock = async function (id) {
    const novoStock = parseInt(document.getElementById(`stock-${id}`).value);

    if (isNaN(novoStock) || novoStock < 0) {
        alert("Por favor insira um valor de stock válido.");
        return;
    }

    try {
        // Construct URL manually since API_URL points to /api/Plantas
        const token = sessionStorage.getItem("token");
        const headers = { "Content-Type": "application/json" };
        if (token) headers["Authorization"] = `Bearer ${token}`;

        const response = await fetch(`https://localhost:7010/api/Stocks/${id}`, {
            method: "PUT",
            headers: headers,
            body: JSON.stringify({ quantidade: novoStock })
        });

        if (response.ok) {
            alert("Stock atualizado com sucesso!");
        } else {
            const errorMsg = await response.text();
            alert("Erro ao atualizar stock: " + errorMsg);
        }
    } catch (err) {
        console.error(err);
        alert("Erro de conexão ao atualizar stock.");
    }
};
