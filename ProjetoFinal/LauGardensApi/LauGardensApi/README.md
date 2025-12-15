# Lau's Garden 

Lau's Garden, uma aplicação web Full Stack dedicada ao comércio eletrónico de plantas. Este projeto foi desenvolvido para oferecer uma experiência de compra intuitiva e uma gestão de inventário eficiente.

## Visão Geral

A app permite aos utilizadores explorar um catálogo variado de plantas, adicionar produtos ao carrinho e realizar encomendas. Para os administradores, fornece ferramentas de "Backoffice" para adicionar, editar e remover plantas, bem como carregar imagens dos produtos.

## Funcionalidades Principais

### Cliente
*   Catálogo Interativo: Visualização de plantas por categorias (Árvores, Rosas, Cactos, etc.).
*   Carrinho de Compras: Gestão local de itens (adicionar, remover, alterar quantidades).
*   Checkout Seguro: Validação rigorosa de dados (NIF, Telemóvel) antes da compra.
*   Métodos de Pagamento:
    *   MB Way;
    *   Transferência;

### Administrador
*   Gestão de Plantas: Adicionar novas plantas com upload de imagem, definir preço, descrição e stock inicial.
*   Controlo de Stock: Visualização de tabela de plantas existentes com opções para adicionar stock ou remover produtos.

### Segurança
*   Autenticação JWT: Sistema de login seguro com Tokens.
*   Controlo de Acesso: Menus e funcionalidades exclusivas para Administradores (ex: "Gerir Plantas").

## Tecnologias Utilizadas

### Frontend
*   HTML5 / CSS: Estrutura e design.
*   Bootstrap 5: Layout responsivo e componentes UI.
*   JavaScript: Lógica de cliente, Fetch API, manipulação do DOM.

### Backend
*   C# / ASP.NET Core Web API: Lógica de servidor.
*   Entity Framework Core: Acesso a dados.
*   SQL Database: Armazenamento persistente.

## Estrutura do Projeto

*   `Frontend/`: Código da interface (HTML, CSS, JS, Imagens).
    *   `paginas/`: Páginas organizadas por funcionalidade (`pagCheckout`, `pagGerirPlantas`, `pagLogin`, etc.).
    *   `javaScript/`: Scripts modulares (`mouseOver.js`, `checkout.js`, `gerirPlantas.js`).
*   `Controllers/`: Controladores da API (`PlantasController`, `StocksController`, `ImagensController`).
*   `Models/`: Definição de dados (`Planta`, `Utilizador`, `Reserva`).

## Documentação e Testes (Swagger)

A API disponibiliza uma interface interativa (Swagger UI) para testar os endpoints.

1.  Certifique-se que o Backend está a correr.
2.  Aceda a: `https://localhost:7010/swagger`
3.  Nesta interface pode:
    *   Verificar todos os endpoints disponíveis.
    *   Consultar os esquemas de dados.
    *   Testar pedidos diretamente no browser.

## Como Executar

1.  Backend:
    *   Abra a solução no Visual Studio ou VS Code.
    *   Configure a *Connection String* para a sua base de dados no `appsettings.json`.
    *   Execute as migrações (se necessário) e inicie a API (IIS Express ou Kestrel).
    *   A API ficará disponível em `https://localhost:7010` (por defeito).

2.  Frontend:
    *   Navegue até à pasta `Frontend`.
    *   Abra o ficheiro `index.html` num browser moderno ou utilize a extensão "Live Server".

## Autores
Hugo Bacalhau e Ricardo Evans, turma CET-TPSI0525
Projeto desenvolvido no âmbito da formação na ATEC.
