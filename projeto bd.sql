CREATE DATABASE registro;
USE registro;
CREATE TABLE user_tb(
	id_user INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
    usuario VARCHAR(20) NOT NULL,
    senha VARCHAR(16) NOT NULL
);

DESCRIBE user_tb;
INSERT INTO user_tb (usuario, senha) VALUES ('admin', '1234');
SELECT * FROM user_tb;
ALTER TABLE user_tb 
ADD COLUMN email VARCHAR(40),
ADD COLUMN documento VARCHAR(21);
ADD COLUMN BOOLEAN verificaInst;


UPDATE user_tb
SET documento = '12345678900'
WHERE usuario = 'admin';

CREATE TABLE SolicitacoesDoacao (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Instituicao VARCHAR(255),
    DescricaoNecessidade TEXT,
    NivelUrgencia VARCHAR(50),
    PreferenciaContato VARCHAR(100),
    DataSolicitacao DATETIME DEFAULT CURRENT_TIMESTAMP
);

/*DROP TABLE IF EXISTS fazerumadoacao;*/
CREATE TABLE FazerUmaDoacao (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Instituicao VARCHAR(255),
    OQueDesejaDoar TEXT,
    EstadoItem VARCHAR(50),
    PreferenciaContato VARCHAR(100),
    DataSolicitacao DATETIME DEFAULT CURRENT_TIMESTAMP
);
