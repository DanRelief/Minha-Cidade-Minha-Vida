CREATE DATABASE IF NOT EXISTS registro;
USE registro;
DROP TABLE IF EXISTS categorias_campanha_tb; 
DROP TABLE IF EXISTS campanhas_tb;
DROP TABLE IF EXISTS user_tb;
DROP TABLE IF EXISTS fazerumadoacao;

CREATE TABLE user_tb (
    id_user INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
    usuario VARCHAR(100) NOT NULL,
    senha VARCHAR(100) NOT NULL,
    email VARCHAR(100) NULL,
    documento VARCHAR(25) UNIQUE NULL, 
    verificaInst TINYINT(1) DEFAULT 0 
);

CREATE TABLE campanhas_tb (
    id_campanha INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    rua VARCHAR(255) NOT NULL,
    cep VARCHAR(10) NOT NULL,
    numero VARCHAR(10) NOT NULL,
    bairro VARCHAR(100) NOT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NOT NULL,
    descricao TEXT,
    id_instituicao VARCHAR(25),
    
    FOREIGN KEY (id_instituicao) REFERENCES user_tb(documento)
);

CREATE TABLE categorias_campanha_tb (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    id_campanha INT,
    nome VARCHAR(50) NOT NULL,
    meta INT NOT NULL,
    unidade VARCHAR(10) NOT NULL,
    atual INT DEFAULT 0,
    FOREIGN KEY (id_campanha) REFERENCES campanhas_tb(id_campanha) ON DELETE CASCADE
);

CREATE TABLE fazerumadoacao (
    id_doacao INT AUTO_INCREMENT PRIMARY KEY,
    Instituicao VARCHAR(255),
    OQueDesejaDoar VARCHAR(255),
    EstadoItem VARCHAR(50),
    PreferenciaContato VARCHAR(100),
    Campanha VARCHAR(255) NULL,
    DocumentoDoador VARCHAR(20) NOT NULL
);