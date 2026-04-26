namespace MCMV.Models
{
    public class SolicitacaoDoacao
    {
        public int Id { get; set; }
        public string? Instituicao { get; set; } 
        public string? DescricaoNecessidade { get; set; }
        public string? NivelUrgencia { get; set; }
        public string? PreferenciaContato { get; set; }
    }
}