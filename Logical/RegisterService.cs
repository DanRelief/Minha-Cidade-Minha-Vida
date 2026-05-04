using MySql.Data.MySqlClient;
using MCMV.Data;
using MCMV.Models;
    
namespace MCMV.Logical
{
    public class RegisterService
    {
        private readonly Database _db;

        // Construtor: O ASP.NET vai entregar o Database configurado aqui
        public RegisterService(Database db)
        {
            _db = db;
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
                    cmd.Parameters.AddWithValue("@mail", email);
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
    }
}