-- DROP DATABASE IF EXISTS LausGarden;

CREATE DATABASE LausGarden;

USE LausGarden;

-- Tabela para guardar os dados de login dos utilizadores 
CREATE TABLE IF NOT EXISTS Utilizadores (
    Id INT NOT NULL AUTO_INCREMENT,
    NomeUtilizador VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(200) NOT NULL,
    Role VARCHAR(20) NOT NULL DEFAULT 'colaborador', 
    PRIMARY KEY (Id)
);
-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Tabela para guardar os dados dos funcionários 
CREATE TABLE IF NOT EXISTS Funcionarios (
    Id INT NOT NULL AUTO_INCREMENT,
    UtilizadorId INT NOT NULL UNIQUE, 
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100),
    Telefone VARCHAR(20),
    Funcao VARCHAR(50),
    PRIMARY KEY (Id),
    CONSTRAINT fk_func_utilizador FOREIGN KEY (UtilizadorId)
        REFERENCES Utilizadores(Id)
        ON DELETE CASCADE 
);
-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Tabela para guardar os dados das Plantas
CREATE TABLE IF NOT EXISTS Plantas (
    Id INT NOT NULL AUTO_INCREMENT,
    Nome VARCHAR(100) NOT NULL,
    Categoria VARCHAR(50) NOT NULL, 
    Preco DECIMAL(10,2) NOT NULL,
    Descricao TEXT,
    UrlImagem VARCHAR(255),
    PRIMARY KEY (Id),
    UNIQUE (Nome, Categoria) -- Não pode haver duas plantas com o mesmo nome e categoria
);
-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Tabela para controlar o Stock das Plantas
CREATE TABLE IF NOT EXISTS Stock (
    Id INT NOT NULL AUTO_INCREMENT,
    PlantaId INT NOT NULL UNIQUE, -- Liga à tabela Plantas (cada planta tem uma linha de stock)
    Quantidade INT NOT NULL DEFAULT 0,
    UltimaAtualizacao TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoPorFuncionarioId INT, 
    PRIMARY KEY (Id),

    CONSTRAINT fk_stock_planta FOREIGN KEY (PlantaId)
        REFERENCES Plantas(Id)
        ON DELETE CASCADE, 

    CONSTRAINT fk_stock_func FOREIGN KEY (AtualizadoPorFuncionarioId)
        REFERENCES Funcionarios(Id)
        ON DELETE SET NULL -- Se o funcionário for apagado, este campo fica NULO
);
-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Tabela para Reservas 
CREATE TABLE IF NOT EXISTS Reservas (
    Id INT NOT NULL AUTO_INCREMENT,
    NomeCliente VARCHAR(100) NOT NULL,
    Contacto VARCHAR(50) NOT NULL,
    PlantaId INT NOT NULL,
    DataReserva TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Status VARCHAR(20) NOT NULL DEFAULT 'Pendente',
    PRIMARY KEY (Id),
    CONSTRAINT fk_reserva_planta FOREIGN KEY (PlantaId)
        REFERENCES Plantas(Id)
        ON DELETE CASCADE
);

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Utilizadores
INSERT INTO Utilizadores (NomeUtilizador, PasswordHash, Role) VALUES
    ('admin', 'admin', 'admin'),
    ('func1', 'func1', 'colaborador');

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Funcionários 
INSERT INTO Funcionarios (UtilizadorId, Nome, Email, Telefone, Funcao) VALUES
(
    (SELECT Id FROM Utilizadores WHERE NomeUtilizador = 'admin'),
    'Administrador Geral',
    'admin@lausgarden.pt',
    '910000000',
    'Administrador'
),
(
    (SELECT Id FROM Utilizadores WHERE NomeUtilizador = 'func1'),
    'Colaborador 1',
    'func1@lausgarden.pt',
    '920000000',
    'Gestor de Stock / Apoio ao Cliente'
);

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Plantas - Rosas
INSERT INTO Plantas (Nome, Categoria, Preco, Descricao, UrlImagem) VALUES
    ('English Miss', 'Rosas', 12.50,
    'Rosa híbrida de chá, cor rosa suave, aroma doce e levemente picante.',
    '/img/pagRosas/Rosas.png'),

    ('Double Delight', 'Rosas', 14.90,
    'Rosa híbrida de chá creme com bordas vermelho-carmim intenso, aroma forte, doce e especiado.',
    '/img/pagRosas/RosaHibridaCha.png'),

    ('Queen Elizabeth', 'Rosas', 16.50,
    'Rosa grandiflora rosa médio, aroma suave, arbusto de grande porte.',
    '/img/pagRosas/QueenElizabeth.png');

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Plantas - Cactos
INSERT INTO Plantas (Nome, Categoria, Preco, Descricao, UrlImagem) VALUES
    ('Echeveria Black Prince', 'Cactos', 6.50,
    'Suculenta/cacto ornamental em roseta compacta, folhas escuras quase pretas.',
    '/img/pagCactos/EcheveriaBlackPrince.png'),

    ('Orelha de Gato', 'Cactos', 5.90,
    'Suculenta ornamental com folhas carnudas e aveludadas, verde-acinzentadas.',
    '/img/pagCactos/OrelhaGato2.png');

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Plantas - Palmeiras
INSERT INTO Plantas (Nome, Categoria, Preco, Descricao, UrlImagem) VALUES
    ('Washingtonia Filifera', 'Palmeiras', 45.00,
    'Palmeira ornamental de grande porte com folhas em leque.',
    '/img/pagPalmeiras/PalmeirasLeque.png'),

    ('Phoenix Canariensis', 'Palmeiras', 55.00,
    'Palmeira das Canárias, muito usada em jardins.',
    '/img/pagPalmeiras/PhoenixCanariensis.png'),

    ('Syagrus', 'Palmeiras', 39.90,
    'Palmeira ornamental/produtiva, tronco delgado e frutos alaranjados.',
    '/img/pagPalmeiras/Coqueiros.png');

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Plantas - Arbustos
INSERT INTO Plantas (Nome, Categoria, Preco, Descricao, UrlImagem) VALUES
    ('Eugénia', 'Arbustos', 12.00,
    'Arbusto perene usado em sebes, produz pequenos frutos vermelhos.',
    '/img/pagArbustos/EugrniaArb.png');

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Plantas - Árvores
INSERT INTO Plantas (Nome, Categoria, Preco, Descricao, UrlImagem) VALUES
    ('Oliveira', 'Árvores', 35.00,
    'Árvore frutífera perene típica do Mediterrâneo.',
    '/img/pagArvores/Oliveira.png'),

    ('Ficus Benjamina', 'Árvores', 29.90,
    'Árvore ornamental perene com folhas verdes brilhantes.',
    '/img/pagArvores/FicusBenjamina.png');

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Plantas - Árvores de Fruto
INSERT INTO Plantas (Nome, Categoria, Preco, Descricao, UrlImagem) VALUES
    ('Figueira', 'Árvores de Fruto', 30.00,
    'Árvore caducifólia que produz figos doces.',
    '/img/pagArvoresFruto/Figueira.png'),

    ('Nespereira', 'Árvores de Fruto', 32.00,
    'Árvore de frutos amarelos (nêsperas) de polpa doce.',
    '/img/pagArvoresFruto/Nespereira.png'),

    ('Golden Delicious', 'Árvores de Fruto', 34.90,
    'Macieira que produz maçãs douradas doces e aromáticas.',
    '/img/pagArvoresFruto/Macieira.png');

-- ----------------------------------------------------------------------------------------------------------------------------------------------
-- Inserir Stock de todas as plantas automaticamente 
INSERT INTO Stock (PlantaId, Quantidade, AtualizadoPorFuncionarioId)
SELECT p.Id,
        CASE
            WHEN p.Categoria = 'Rosas' THEN 20
            WHEN p.Categoria = 'Cactos' THEN 30
            WHEN p.Categoria = 'Suculentas' THEN 35
            WHEN p.Categoria = 'Palmeiras' THEN 8
            WHEN p.Categoria = 'Arbustos' THEN 20
            WHEN p.Categoria = 'Árvores' THEN 12
            WHEN p.Categoria = 'Árvores de Fruto' THEN 10
            ELSE 5 -- Quantidade predefinida para outras categorias
        END,
        (SELECT Id FROM Funcionarios WHERE Nome = 'Colaborador 1')
FROM Plantas p;