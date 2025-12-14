
// Carregar o carrinho ao iniciar a página
document.addEventListener("DOMContentLoaded", function () {
    if (!sessionStorage.getItem("token")) {
        alert("Tem de estar logado para aceder ao checkout.");
        window.location.href = "../pagLogin/login.html";
        return;
    }

    carregarCarrinho();

    // Associar evento ao formulário de checkout
    const form = document.querySelector("form");
    if (form) {
        form.addEventListener("submit", finalizarCompra);
    }
});

function carregarCarrinho() {
    const carrinho = JSON.parse(localStorage.getItem('lausGardenCart')) || [];
    const container = document.getElementById("itensCarrinho");
    const resumoLista = document.querySelector(".list-group.mb-3");

    if (!container && !resumoLista) return;

    let total = 0;

    // Limpar lista de resumo (vamos reconstruir)
    if (resumoLista) resumoLista.innerHTML = "";

    if (carrinho.length === 0) {
        if (resumoLista) {
            resumoLista.innerHTML = "<li class='list-group-item text-center'>O seu carrinho está vazio.</li>";
        }
        return;
    }

    carrinho.forEach(item => {
        const subtotal = item.preco * item.quantidade;
        total += subtotal;

        // Adicionar item ao resumo da compra
        if (resumoLista) {
            const li = document.createElement("li");
            li.className = "list-group-item d-flex justify-content-between lh-sm align-items-center";
            li.innerHTML = `
                <div>
                    <h6 class="my-0">${item.nome}</h6>
                    <small class="text-muted">
                        <div class="input-group input-group-sm" style="width: 100px; margin-top: 5px;">
                            <button class="btn btn-outline-secondary" type="button" onclick="atualizarQuantidade(${item.id}, -1)">-</button>
                            <input type="text" class="form-control text-center" value="${item.quantidade}" readonly>
                            <button class="btn btn-outline-secondary" type="button" onclick="atualizarQuantidade(${item.id}, 1)">+</button>
                        </div>
                    </small>
                </div>
                <div class="text-end">
                    <span class="text-muted">${subtotal.toFixed(2)} €</span><br>
                    <a href="#" onclick="removerItem(${item.id})" class="text-danger" style="font-size: 0.8rem; text-decoration: none;">Remover</a>
                </div>
            `;
            resumoLista.appendChild(li);
        }
    });

    // Adicionar Linha de Total
    if (resumoLista) {
        const liTotal = document.createElement("li");
        liTotal.className = "list-group-item d-flex justify-content-between";
        liTotal.innerHTML = `
            <span>Total (EUR)</span>
            <strong>${total.toFixed(2)} €</strong>
        `;
        resumoLista.appendChild(liTotal);
    }
}

function atualizarQuantidade(id, delta) {
    let carrinho = JSON.parse(localStorage.getItem('lausGardenCart')) || [];
    const item = carrinho.find(i => i.id === id);

    if (item) {
        item.quantidade += delta;
        if (item.quantidade <= 0) {
            removerItem(id);
            return;
        } else {
            localStorage.setItem('lausGardenCart', JSON.stringify(carrinho));
        }
    }
    carregarCarrinho();
}

function removerItem(id) {
    let carrinho = JSON.parse(localStorage.getItem('lausGardenCart')) || [];
    carrinho = carrinho.filter(i => i.id !== id);
    localStorage.setItem('lausGardenCart', JSON.stringify(carrinho));
    carregarCarrinho();
}

async function finalizarCompra(event) {
    event.preventDefault();

    const carrinho = JSON.parse(localStorage.getItem('lausGardenCart')) || [];
    if (carrinho.length === 0) {
        alert("O seu carrinho está vazio!");
        return;
    }

    const checkoutItems = carrinho.map(item => ({
        PlantaId: item.id,
        Quantidade: item.quantidade
    }));

    // URL da API
    const API_URL = "https://localhost:7010/api/Stocks/Checkout";

    try {
        const token = sessionStorage.getItem("token");
        const headers = {
            "Content-Type": "application/json"
        };

        if (token) {
            headers["Authorization"] = `Bearer ${token}`;
        }

        const response = await fetch(API_URL, {
            method: "POST",
            headers: headers,
            body: JSON.stringify(checkoutItems)
        });

        if (response.ok) {
            alert("Pagamento realizado com sucesso, as suas plantas estarão à sua espera!");
            localStorage.removeItem('lausGardenCart');
            window.location.href = "../index.html";
        } else {
            const errorMsg = await response.text();
            alert("Erro ao finalizar compra: " + errorMsg);
        }
    } catch (error) {
        console.error("Erro na requisição:", error);
        alert("Erro de conexão com o servidor. Verifique se a API está a correr.");
    }
}
