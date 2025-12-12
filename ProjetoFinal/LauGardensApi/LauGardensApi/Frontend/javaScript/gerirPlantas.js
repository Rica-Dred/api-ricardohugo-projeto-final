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

        plantas.forEach(p => {
            const tr = document.createElement("tr");

            // Safe URL handling
            let imgUrl = p.urlImagem;
            if (imgUrl && !imgUrl.startsWith("http") && !imgUrl.startsWith("/")) {
                imgUrl = "../" + imgUrl; // Attempt relative adjustment if needed
            }

            tr.innerHTML = `
                <td><img src="${imgUrl}" alt="${p.nome}" style="width: 50px; height: 50px; object-fit: cover; border-radius: 5px;"></td>
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
                    <button class="btn btn-sm btn-outline-danger" onclick="removerPlanta(${p.id}, '${p.nome}')">
                        <i class="bi bi-trash"></i> Apagar
                    </button>
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

        const novaPlanta = {
            nome: document.getElementById("nome").value,
            categoria: document.getElementById("categoria").value,
            preco: parseFloat(document.getElementById("preco").value),
            quantidadeInicial: parseInt(document.getElementById("quantidadeInicial").value),
            urlImagem: document.getElementById("urlImagem").value,
            descricao: document.getElementById("descricao").value
        };

        try {
            const response = await fetch(API_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(novaPlanta)
            });

            if (response.ok) {
                alert("Planta adicionada com sucesso!");
                form.reset();
                carregarPlantas(); // Refresh list
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
    if (!confirm(`Tem a certeza que deseja eliminar a planta "${nome}"? Esta ação é irreversível.`)) {
        return;
    }

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: "DELETE"
        });

        if (response.ok || response.status === 204) {
            alert("Planta eliminada com sucesso.");
            carregarPlantas(); // Refresh list
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
        const response = await fetch(`https://localhost:7010/api/Stocks/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ quantidade: novoStock })
        });

        if (response.ok) {
            alert("Stock atualizado com sucesso!");
            // No need to reload everything, value is already there
        } else {
            const errorMsg = await response.text();
            alert("Erro ao atualizar stock: " + errorMsg);
        }
    } catch (err) {
        console.error(err);
        alert("Erro de conexão ao atualizar stock.");
    }
};
