using System;
using System.Collections.Generic;
using MCMV.Models;
using Microsoft.Extensions.Configuration; 
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;

namespace MCMV.Logical
{
    public class DonationService
    {
        private readonly string _connectionString;

        public DonationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public void SalvarSolicitacao(SolicitacaoDoacao solicitacao)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            // Alterado os nomes das colunas para bater com o seu banco de dados correto
            const string sql = "INSERT INTO solicitacaodoacao (nome_user, descricao_necessidade, nivel_urgencia, contato) " +
                               "VALUES (@inst, @desc, @urg, @contato)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inst", solicitacao.NomeUser);
            cmd.Parameters.AddWithValue("@desc", solicitacao.DescricaoNecessidade);
            cmd.Parameters.AddWithValue("@urg", solicitacao.NivelUrgencia);
            cmd.Parameters.AddWithValue("@contato", solicitacao.Contato);
            cmd.ExecuteNonQuery();
        }


        public void SalvarOfertaDoacao(FazerUmaDoacao doacao, string documentoUsuarioLogado)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = @"INSERT INTO fazerumadoacao 
                        (Instituicao, OQueDesejaDoar, EstadoItem, PreferenciaContato, Campanha, DocumentoDoador) 
                        VALUES (@inst, @oque, @estado, @contato, @camp, @doador)";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@inst", doacao.Instituicao ?? "Nulo");
            cmd.Parameters.AddWithValue("@oque", doacao.OQueDesejaDoar ?? "Nulo");
            cmd.Parameters.AddWithValue("@estado", doacao.EstadoItem ?? "Nulo");
            cmd.Parameters.AddWithValue("@contato", doacao.Contato ?? "Nulo");
            cmd.Parameters.AddWithValue("@doador", documentoUsuarioLogado);

            if (string.IsNullOrWhiteSpace(doacao.Campanha))
            {
                cmd.Parameters.AddWithValue("@camp", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@camp", doacao.Campanha);
            }

            cmd.ExecuteNonQuery();
        }

        public List<CategoriaCampanhaModel> ListarCategoriasPorCampanha(int idCampanha)
        {
            var categorias = new List<CategoriaCampanhaModel>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT id_categoria AS Id, 
                              id_campanha AS CampanhaId, 
                              nome_categoria AS Nome, 
                              meta_categoria AS Meta, 
                              atual_categoria AS Atual, 
                              unidade_categoria AS Unidade 
                       FROM categorias_campanha_tb 
                       WHERE id_campanha = @id";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", idCampanha);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    categorias.Add(new CategoriaCampanhaModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CampanhaId = Convert.ToInt32(reader["CampanhaId"]),
                        Nome = reader["Nome"].ToString(),
                        Meta = Convert.ToInt32(reader["Meta"]),
                        Atual = Convert.ToInt32(reader["Atual"]),
                        Unidade = reader["Unidade"].ToString()
                    });
                }
            }
            return categorias;
        }

        public MeusDadosViewModel ObterResumoUsuario(string documento)
        {
            var resumo = new MeusDadosViewModel();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string nomeCompletoUsuario = "";
            const string sqlBuscarNome = "SELECT usuario FROM user_tb WHERE documento = @doc LIMIT 1";

            using (var cmdNome = new MySqlCommand(sqlBuscarNome, conn))
            {
                cmdNome.Parameters.AddWithValue("@doc", documento.Trim());
                var resultado = cmdNome.ExecuteScalar();
                if (resultado != null)
                {
                    nomeCompletoUsuario = resultado.ToString().Trim();
                }
            }

            string primeiroNome = nomeCompletoUsuario.Split(' ')[0];

            const string sqlSolicitadas = @"
        SELECT COUNT(*) 
        FROM solicitacaodoacao 
        WHERE LOWER(nome_user) COLLATE utf8mb4_general_ci LIKE LOWER(@nomeUser) COLLATE utf8mb4_general_ci";

            const string sqlEnviadas = "SELECT COUNT(*) FROM fazerumadoacao WHERE DocumentoDoador = @doc";
            const string sqlEspontaneas = "SELECT COUNT(*) FROM fazerumadoacao WHERE DocumentoDoador = @doc AND (Campanha IS NULL OR Campanha = '' OR Campanha = 'Doação Avulsa')";
            const string sqlInst = "SELECT DISTINCT Instituicao FROM fazerumadoacao WHERE DocumentoDoador = @doc";
            const string sqlCampanhas = @"
        SELECT COUNT(DISTINCT Campanha) 
        FROM fazerumadoacao 
        WHERE DocumentoDoador = @doc 
          AND Campanha IS NOT NULL 
          AND Campanha != '' 
          AND Campanha != 'Doação Avulsa'";

            using (var cmd = new MySqlCommand(sqlSolicitadas, conn))
            {
                cmd.Parameters.AddWithValue("@nomeUser", primeiroNome + "%");
                resumo.DoacoesSolicitadas = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Contabiliza as Doações Enviadas
            using (var cmd = new MySqlCommand(sqlEnviadas, conn))
            {
                cmd.Parameters.AddWithValue("@doc", documento.Trim());
                resumo.DoacoesEnviadas = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Contabiliza as Doações Espontâneas
            using (var cmd = new MySqlCommand(sqlEspontaneas, conn))
            {
                cmd.Parameters.AddWithValue("@doc", documento.Trim());
                resumo.DoacoesEspontaneas = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Contabiliza as Campanhas Participadas
            using (var cmd = new MySqlCommand(sqlCampanhas, conn))
            {
                cmd.Parameters.AddWithValue("@doc", documento.Trim());
                resumo.CampanhasParticipadas = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // Alimenta a lista de Instituições Contatadas
            using (var cmd = new MySqlCommand(sqlInst, conn))
            {
                cmd.Parameters.AddWithValue("@doc", documento.Trim());
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        resumo.InstituicoesContatadas.Add(reader.GetString(0));
                    }
                }
            }

            return resumo;
        }

        public List<InstituicaoTransparencia> ObterDadosPortal()
        {
            var lista = new List<InstituicaoTransparencia>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string sql = @"
        SELECT 
            u.usuario as Nome, 
            u.documento as Documento,
            u.email as Email,
            (
                SELECT COUNT(*) 
                FROM fazerumadoacao f 
                WHERE f.Instituicao = u.usuario 
                  AND f.EstadoItem != 'Nulo' 
                  AND f.EstadoItem != '' 
                  AND f.EstadoItem IS NOT NULL
            ) as TotalItens,
            (
                SELECT IFNULL(SUM(
                    CAST(
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(f.OQueDesejaDoar, 'R$', ''), 
                                ' ', ''), 
                            '.', ''), 
                        ',', '.') 
                    AS DECIMAL(10,2))
                ), 0)
                FROM fazerumadoacao f
                WHERE f.Instituicao = u.usuario 
                  AND (f.EstadoItem = 'Nulo' OR f.EstadoItem = '' OR f.EstadoItem IS NULL)
                  AND f.OQueDesejaDoar REGEXP '[0-9]'
            ) as TotalDinheiro
        FROM user_tb u
        WHERE u.verificaInst = 1
        ORDER BY u.usuario ASC";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var item = new InstituicaoTransparencia();
                item.Nome = reader.GetString("Nome");
                item.Documento = reader.GetString("Documento");
                item.Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString("Email");

                item.TotalItensRecebidos = reader.GetInt32("TotalItens");
                item.TotalArrecadado = reader.GetDecimal("TotalDinheiro");

                lista.Add(item);
            }
            return lista;
        }
    }
}