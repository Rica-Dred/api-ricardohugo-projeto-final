function mouseOver(el) {
    el.style.color = "rgb(221, 161, 94)";
}

function mouseOut(el) {
    el.style.color = "";
}

// LÓGICA DE LOGIN/LOGOUT (Adicionada para gerir o menu dinamicamente)
document.addEventListener("DOMContentLoaded", () => {
    // 1. Encontrar o link que diz "Log Out" (ou "Login" se já tiver sido alterado manualmente)
    const navLinks = document.querySelectorAll(".nav-link span");
    let authLink = null;
    let authLinkContainer = null;

    navLinks.forEach(span => {
        if (span.innerText.trim().toUpperCase() === "LOG OUT" || span.innerText.trim().toUpperCase() === "LOGIN") {
            authLink = span;
            authLinkContainer = span.closest('a'); // O elemento <a> pai
        }
    });

    if (!authLink || !authLinkContainer) return;

    // 2. Verificar se está logado
    // 2. Verificar se está logado
    const token = sessionStorage.getItem("token");
    const username = sessionStorage.getItem("username");

    if (token) {
        // ESTÁ LOGADO
        // Mostra o nome do user + Logout
        // Proteção contra "undefined" string (caso o backend não tenha sido atualizado)
        const nomeParaMostrar = (username && username !== "undefined" && username !== "null") ? username : "Conta";
        authLink.innerHTML = `<i class="bi bi-person-circle"></i> ${nomeParaMostrar} | Sair`;

        // Ao clicar, faz logout
        authLinkContainer.href = "#";
        authLinkContainer.onclick = (e) => {
            e.preventDefault();
            if (confirm(`Deseja sair da conta de ${nomeParaMostrar}?`)) {
                sessionStorage.removeItem("token");
                sessionStorage.removeItem("role");
                sessionStorage.removeItem("username");
                alert("Sessão terminada.");
                // Ajustar caminho de redirecionamento com base na localização atual
                if (window.location.pathname.includes("/paginas/pag")) {
                    // Estamos numa sub-pasta (ex: paginas/pagCactos/cactos.html) -> Subir um nível
                    window.location.href = "../index.html";
                } else {
                    // Estamos na raiz de paginas (ex: login.html) -> Irmão
                    window.location.href = "index.html";
                }
            }
        };

    } else {
        // NÃO ESTÁ LOGADO
        authLink.innerText = "Login";

        // Ajustar o link para ir para a página de login
        // Detetar se estamos numa subpasta (../login.html) ou na raiz (./login.html)
        const currentPath = window.location.pathname;
        // Vamos confiar no href que já lá está no HTML, só mudamos o texto.
        // O código anterior que forçava o caminho estava a causar duplicados (Frontend/Frontend).
    }
    // 3. Gerir Visibilidade do Menu "Gerir Plantas"
    // Limpar localStorage antigo para evitar conflitos (opcional, mas recomendado já que mudámos para session)
    localStorage.removeItem("token");
    localStorage.removeItem("role");

    const role = sessionStorage.getItem("role");
    // Debug inicial
    console.log("[DEBUG] Verificando Visibilidade. Role (Session):", role);

    const navItems = document.querySelectorAll(".nav-link span");
    console.log("[DEBUG] Itens de navegação encontrados:", navItems.length);

    navItems.forEach(span => {
        // Debug do item
        // console.log("[DEBUG] Item encontrado:", span.innerText);

        if (span.innerText.trim() === "Gerir Plantas") {
            const linkContainer = span.closest('li') || span.closest('a');

            console.log("[DEBUG] 'Gerir Plantas' encontrado. Role atual:", role);

            // Só mostra se for admin ou func
            if (role === "admin" || role === "func") {
                console.log("[DEBUG] -> A mostrar link (Admin/Func)");
                if (linkContainer) {
                    linkContainer.style.display = "block";
                    linkContainer.style.removeProperty('display');
                }
            } else {
                console.log("[DEBUG] -> A esconder link (Não autorizado)");
                if (linkContainer) linkContainer.style.display = "none";
            }
        }
    });

});