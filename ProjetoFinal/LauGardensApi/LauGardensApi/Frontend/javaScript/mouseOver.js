function mouseOver(el) {
    el.style.color = "rgb(221, 161, 94)";
}

function mouseOut(el) {
    el.style.color = "";
}

// LÓGICA DE LOGIN/LOGOUT (Adicionada para gerir o menu dinamicamente)
document.addEventListener("DOMContentLoaded", () => {
    // 1. Encontrar o link que diz "Log Out" (ou "Login")
    const navLinks = document.querySelectorAll(".nav-link span");
    let authLink = null;
    let authLinkContainer = null;

    navLinks.forEach(span => {
        const text = span.innerText.replace(/\s+/g, ' ').trim().toUpperCase();
        if (text === "LOG OUT" || text === "LOGIN") {
            authLink = span;
            authLinkContainer = span.closest('a');
        }
    });

    if (!authLink || !authLinkContainer) return;

    // 2. Verificar se está logado
    const token = sessionStorage.getItem("token");
    const username = sessionStorage.getItem("username");

    if (token) {
        // ESTÁ LOGADO
        const nomeParaMostrar = (username && username !== "undefined" && username !== "null") ? username : "Conta";
        authLink.innerHTML = `Sair`;

        // Ao clicar, faz logout
        authLinkContainer.href = "#";
        authLinkContainer.onclick = (e) => {
            e.preventDefault();
            if (confirm(`Deseja sair da conta de ${nomeParaMostrar}?`)) {
                sessionStorage.removeItem("token");
                sessionStorage.removeItem("role");
                sessionStorage.removeItem("username");
                alert("Sessão terminada.");
                // Ajustar caminho de redirecionamento
                const path = window.location.pathname;
                if (path.endsWith("/index.html") || path.endsWith("/paginas/") || path.endsWith("/paginas")) {
                    window.location.href = "index.html";
                } else {
                    window.location.href = "../index.html";
                }
            }
        };

    } else {
        // NÃO ESTÁ LOGADO
        authLink.innerText = "Login";
    }

    // 3. Gerir Visibilidade do Menu "Gerir Plantas"
    // Nota: Removendo limpeza de localStorage pois estamos a usar sessionStorage

    const role = sessionStorage.getItem("role");

    // Debug
    // console.log("[DEBUG] Verificando Visibilidade. Role:", role);

    const navItems = document.querySelectorAll(".nav-link span");

    navItems.forEach(span => {
        const text = span.innerText.replace(/\s+/g, ' ').trim();

        if (text === "Gerir Plantas") {
            const linkContainer = span.closest('li') || span.closest('a');

            // Só mostra se for admin ou func
            if (role === "admin" || role === "func") {
                if (linkContainer) {
                    linkContainer.style.display = "block";
                    linkContainer.style.removeProperty('display');
                }
            } else {
                if (linkContainer) linkContainer.style.display = "none";
            }
        }
    });
});