DROP TABLE user_tb;
CREATE TABLE user_tb (
    id_user INT AUTO_INCREMENT PRIMARY KEY NOT NULL,
    usuario VARCHAR(100) NOT NULL,
    senha VARCHAR(100) NOT NULL,
    email VARCHAR(100) NULL,
    documento VARCHAR(25) NULL,
    verificaInst TINYINT(1) DEFAULT 0 
);