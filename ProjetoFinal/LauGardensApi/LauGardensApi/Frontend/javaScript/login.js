// URL da API
const API_URL = "https://localhost:7010/api/ControllerAutenticacao/login";

document.addEventListener("DOMContentLoaded", () => {

    const form = document.getElementById("formLogin");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const user = document.getElementById("inputUser").value;
        const password = document.getElementById("inputPass").value;

        const request = {
            nomeUtilizador: user,
            password: password
        };

        try {
            const response = await fetch(API_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(request)
            });

            if (!response.ok) {
                alert("Email ou palavra-passe incorretos!");
                return;
            }

            const data = await response.json();

            // Guardar o token no localStorage
            localStorage.setItem("token", data.token);
            localStorage.setItem("role", data.role);

            alert("Login bem-sucedido!");

            // Redireciona para página principal
            window.location.href = "../paginas/index.html";

        } catch (error) {
            console.error("Erro no login:", error);
            alert("Erro ao ligar ao servidor.");
        }
    });
});