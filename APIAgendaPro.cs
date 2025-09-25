using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;

namespace AgendaProApp
{
    public class APIAgendaPro
    {
        private readonly HttpClient _client;
        private string? _token;

        public APIAgendaPro(string baseUrl)
        {
            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        public async Task<bool> AgendaProLogin(string usuario, string senha)
        {
            try
            {
                var loginData = new { login = usuario, senha = senha };

                using var response = await _client.PostAsync(
                    "auth/login",
                    new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json")
                ).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                using var doc = JsonDocument.Parse(content);

                _token = doc.RootElement.GetProperty("token").GetString();

                if (!string.IsNullOrEmpty(_token))
                {
                    _client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // mantém o mesmo comportamento: mostra erro em MessageBox
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Erro ao autenticar: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return false;
            }
        }

        public async Task<JsonDocument?> AgendaProPost(string rota, object body)
        {
            try
            {
                using var response = await _client.PostAsync(
                    rota,
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                ).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonDocument.Parse(content);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Erro POST {rota}: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return null;
            }
        }

        public async Task<JsonDocument?> AgendaProPut(string rota, object body)
        {
            try
            {
                string jsonString = body is JsonObject jo
                    ? jo.ToJsonString()
                    : JsonSerializer.Serialize(body);

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                //MessageBox.Show($"PUT {rota} com body: {jsonString}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                var response = await _client.PutAsync(rota, content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseString))
                    return null;

                return JsonDocument.Parse(responseString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro PUT {rota}: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }




        public async Task<JsonDocument?> AgendaProGet(string rota)
        {
            try
            {
                using var response = await _client.GetAsync(rota).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonDocument.Parse(content);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"Erro GET {rota}: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                });
                return null;
            }
        }
    }
}
