namespace MCMV.Models
{
    public class CampanhaModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Rua { get; set; }
        public string Cep { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string? Descricao { get; set; }
        public string DocumentoInstituicao { get; set; }
    }

    public class CategoriaCampanhaModel
    {
        public int Id { get; set; }
        public int CampanhaId { get; set; }
        public string Nome { get; set; }
        public int Meta { get; set; }
        public int Atual { get; set; } // Adicione esta linha
        public string Unidade { get; set; }
    }
}