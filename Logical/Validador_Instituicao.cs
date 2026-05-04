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

            try
            {
                var response = await http.GetAsync($"osc?ft_identificador_osc={cnpj}");

                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();

                using var resultado = JsonDocument.Parse(json);
                var root = resultado.RootElement;

                return root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0;
            }
            catch
            {
                // Em caso de erro na API (timeout ou fora do ar), você decide se barra 
                // ou se permite o cadastro. Aqui retornaremos false para segurança.
                return false;
            }
        }
    }
}