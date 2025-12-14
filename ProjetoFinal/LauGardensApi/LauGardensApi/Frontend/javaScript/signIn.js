// URL da API
const API_URL = "https://localhost:7010/api/Utilizador";

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("formSignIn");

    if (form) {
        form.addEventListener("submit", async (e) => {
            e.preventDefault();

            // Password Validation (Optional but good practice)
            const password = document.getElementById("inputPass").value;
            const confirmPassword = document.getElementById("inputConfirmPass").value;

            if (password !== confirmPassword) {
                alert("As passwords não coincidem!");
                return;
            }

            const novoUtilizador = {
                NomeUtilizador: document.getElementById("inputUser").value,
                PasswordHash: password, // In plain text as per current system design
                Role: "cliente" // Force role
            };

            try {
                const response = await fetch(API_URL, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify(novoUtilizador)
                });

                if (response.ok) {
                    alert("Conta criada com sucesso! Por favor faça login.");
                    window.location.href = "../pagLogin/login.html";
                } else {
                    const errorMsg = await response.text();
                    alert("Erro ao criar conta: " + errorMsg);
                }
            } catch (error) {
                console.error("Erro:", error);
                alert("Erro de conexão com o servidor.");
            }
        });
    }
});
