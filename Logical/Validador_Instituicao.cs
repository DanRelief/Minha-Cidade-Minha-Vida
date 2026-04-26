using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCMV.Logical
{
    public static class Validador_Instituicao
    {
        private static readonly HttpClient http = new HttpClient
        {
            BaseAddress = new System.Uri("https://mapaosc.ipea.gov.br/api/")
        };

        public static async Task<bool> VerificarInstituicao(string documento)
        {
            string cnpj = Regex.Replace(documento, @"\D", "");

            if (cnpj.Length != 14)
                return false;

            var response = await http.GetAsync($"osc?ft_identificador_osc={cnpj}");
            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<JsonElement>(json);

            return resultado.ValueKind == JsonValueKind.Array &&
                   resultado.GetArrayLength() > 0;
        }
    }
}