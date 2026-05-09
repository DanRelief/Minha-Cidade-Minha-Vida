using MCMV.Data;
using MCMV.Models;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace MCMV.Logical
{
    public class RegisterService
    {
        private readonly string _connectionString;

        private readonly Database _db;

        // Construtor: O ASP.NET vai entregar o Database configurado aqui
        public RegisterService(Database db, IConfiguration configuration)
        {
            _db = db;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool UsuarioExiste(string documento)
        {
            using (var con = _db.GetConnection())
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM user_tb WHERE documento = @doc";

                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@doc", documento);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public void CriarUsuario(string user, string senha, string? email, string doc, bool instituicaoVerificada)
        {
            string emailTratado = email ?? "nao-informado@email.com";
            using (var con = _db.GetConnection())
            {
                con.Open();
                string query = "INSERT INTO user_tb (usuario, senha, email, documento, verificaInst) VALUES (@user, @pass, @mail, @doc, @verificaInst)";

                using (var cmd = new MySqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@user", user);
                    cmd.Parameters.AddWithValue("@pass", senha);
                    cmd.Parameters.AddWithValue("@mail", emailTratado);
                    cmd.Parameters.AddWithValue("@doc", doc);
                    cmd.Parameters.AddWithValue("@verificaInst", instituicaoVerificada);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<UserViewModel> ListarInstituicoes()
        {
            var lista = new List<UserViewModel>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                const string sql = "SELECT usuario, documento FROM user_tb WHERE LENGTH(documento) = 14";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new UserViewModel
                            {
                                // Certifique-se que os nomes aqui batem com o SELECT
                                Nome = reader["usuario"].ToString() ?? "",
                                Documento = reader["documento"].ToString() ?? ""
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public bool mudarValidacaoInstituicao(string cnpj)
        {
            try
            {
                using (var con = _db.GetConnection())
                {
                    con.Open();
                    string query = "UPDATE user_tb SET verificaInst = true WHERE documento = @cnpj";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@cnpj", cnpj);
                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        // Retorna true se pelo menos uma linha foi alterada
                        return linhasAfetadas > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Não foi possivel atualizar");
                return false;
            }
        }

        public void RegistrarCampanha(CampanhaModel camp, List<CategoriaCampanhaModel> categorias)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            using var trans = conn.BeginTransaction();

            try
            {
                // 1. Insere a Campanha
                string sqlCamp = @"INSERT INTO campanhas_tb (nome, rua, cep, numero, bairro, data_inicio, data_fim, descricao, id_instituicao) 
                           VALUES (@nome, @rua, @cep, @num, @bairro, @ini, @fim, @desc, @inst);
                           SELECT LAST_INSERT_ID();";

                using var cmdCamp = new MySqlCommand(sqlCamp, conn, trans);
                cmdCamp.Parameters.AddWithValue("@nome", camp.Nome);
                cmdCamp.Parameters.AddWithValue("@rua", camp.Rua);
                cmdCamp.Parameters.AddWithValue("@cep", camp.Cep);
                cmdCamp.Parameters.AddWithValue("@num", camp.Numero);
                cmdCamp.Parameters.AddWithValue("@bairro", camp.Bairro);
                cmdCamp.Parameters.AddWithValue("@ini", camp.DataInicio);
                cmdCamp.Parameters.AddWithValue("@fim", camp.DataFim);
                cmdCamp.Parameters.AddWithValue("@desc", camp.Descricao ?? (object)DBNull.Value);
                cmdCamp.Parameters.AddWithValue("@inst", camp.DocumentoInstituicao);

                int campanhaId = Convert.ToInt32(cmdCamp.ExecuteScalar());

                // 2. Insere as Categorias
                string sqlCat = @"INSERT INTO categorias_campanha_tb (id_campanha, nome, meta, unidade) 
                          VALUES (@campId, @nome, @meta, @unidade)";

                foreach (var cat in categorias)
                {
                    using var cmdCat = new MySqlCommand(sqlCat, conn, trans);
                    cmdCat.Parameters.AddWithValue("@campId", campanhaId);
                    cmdCat.Parameters.AddWithValue("@nome", cat.Nome);
                    cmdCat.Parameters.AddWithValue("@meta", cat.Meta);
                    cmdCat.Parameters.AddWithValue("@unidade", cat.Unidade);
                    cmdCat.ExecuteNonQuery();
                }

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public List<CampanhaModel> ListarCampanhasPorInstituicao(string documento)
        {
            var lista = new List<CampanhaModel>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            // Buscamos as campanhas filtrando pelo documento da instituição
            string sql = "SELECT * FROM campanhas_tb WHERE id_instituicao = @doc ORDER BY data_inicio DESC";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@doc", documento);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new CampanhaModel
                {
                    Id = Convert.ToInt32(reader["id_campanha"]),
                    Nome = reader["nome"].ToString(),
                    DataInicio = Convert.ToDateTime(reader["data_inicio"]),
                    DataFim = Convert.ToDateTime(reader["data_fim"]),
                    // Adicione os outros campos se necessário
                });
            }
            return lista;
        }
    }
}