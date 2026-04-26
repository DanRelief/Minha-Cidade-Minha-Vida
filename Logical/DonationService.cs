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
    }
}