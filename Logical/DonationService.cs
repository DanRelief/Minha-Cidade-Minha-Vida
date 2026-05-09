using MySql.Data.MySqlClient;
using MCMV.Models;

namespace MCMV.Logical
{
    public class DonationService
    {
        private readonly string _connectionString;

        public DonationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // MÉTODO EXISTENTE (Para a aba "Precisa de Doação")
        public void SalvarSolicitacao(SolicitacaoDoacao solicitacao)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            const string sql = "INSERT INTO SolicitacoesDoacao (Instituicao, DescricaoNecessidade, NivelUrgencia, PreferenciaContato) " +
                               "VALUES (@inst, @desc, @urg, @contato)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@inst", solicitacao.Instituicao);
            cmd.Parameters.AddWithValue("@desc", solicitacao.DescricaoNecessidade);
            cmd.Parameters.AddWithValue("@urg", solicitacao.NivelUrgencia);
            cmd.Parameters.AddWithValue("@contato", solicitacao.PreferenciaContato);
            cmd.ExecuteNonQuery();
        }

        // NOVO MÉTODO (Para a aba "Fazer uma Doação")
        // Adaptado para o seu Model: FazerUmaDoacao
        public void SalvarOfertaDoacao(FazerUmaDoacao doacao)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            const string sql = "INSERT INTO fazerumadoacao (Instituicao, OQueDesejaDoar, EstadoItem, PreferenciaContato) " +
                               "VALUES (@inst, @oque, @estado, @contato)";

            using var cmd = new MySqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("@inst", doacao.Instituicao ?? "Nulo");
            cmd.Parameters.AddWithValue("@oque", doacao.OQueDesejaDoar ?? "Nulo");
            cmd.Parameters.AddWithValue("@estado", doacao.EstadoItem ?? "Nulo");
            cmd.Parameters.AddWithValue("@contato", doacao.PreferenciaContato ?? "Nulo");

            cmd.ExecuteNonQuery();
        }

        // Exemplo de como deve ser a implementação dentro do seu DonationService
        public List<CategoriaCampanhaModel> ListarCategoriasPorCampanha(int idCampanha)
        {
            var categorias = new List<CategoriaCampanhaModel>();

            // Exemplo de lógica de conexão (Ajuste para o seu _context ou connection string)
            using (var conn = new MySqlConnection(_connectionString))
            {
                // Certifique-se de que os nomes das colunas (AS Nome, AS Meta...) 
                // coincidem com as propriedades do seu CategoriaCampanhaModel
                string sql = @"SELECT id_categoria AS Id, 
                              id_campanha AS CampanhaId, 
                              nome_categoria AS Nome, 
                              meta_categoria AS Meta, 
                              atual_categoria AS Atual, 
                              unidade_categoria AS Unidade 
                       FROM categorias_campanha_tb 
                       WHERE id_campanha = @id";

                // Se estiver usando Dapper:
                // return conn.Query<CategoriaCampanhaModel>(sql, new { id = idCampanha }).ToList();

                // Se estiver usando ADO.NET puro, você faria o preenchimento manual da lista aqui.
                return categorias;
            }
        }
    }
}