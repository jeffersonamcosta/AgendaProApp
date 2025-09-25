using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AgendaProApp
{
    public partial class Principal
    {
        
        private static readonly Regex regexHoraValida = new Regex(@"^[0-9:]+$");

        private void MostrarTela(Grid telaVisivel)
        {
            TELA_Participantes.Visibility = Visibility.Hidden;
            TELA_Fornecedores.Visibility = Visibility.Hidden;
            TELA_Eventos.Visibility = Visibility.Hidden;
            telaVisivel.Visibility = Visibility.Visible;
        }

        private void BtTelaParticipantes_Click(object sender, RoutedEventArgs e)
        {
            MostrarTela(TELA_Participantes);
            CarregarTiposParticipante();
        }

        private void BtTelaFornecedores_Click(object sender, RoutedEventArgs e)
        {
            TELA_Fornecedor_Servicos_DTG_inciar();
            MostrarTela(TELA_Fornecedores);
        }

        private void BtTelaEventos_Click(object sender, RoutedEventArgs e)
        {
            MostrarTela(TELA_Eventos);
            CarregarTiposEvento();
        }

        private void BtTelaRelatorios_Click(object sender, RoutedEventArgs e) { }

        private string ObterStringDataTime(string date, string time)
        {
            if (string.IsNullOrWhiteSpace(date))
                return null;

            if (string.IsNullOrWhiteSpace(time))
                time = "00:00";

            if (DateTime.TryParse($"{date} {time}", out DateTime resultado))
            {
                return resultado.ToString("yyyy-MM-ddTHH:mm:ss");
            }

            return date;
        }



        private void Hora_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !regexHoraValida.IsMatch(e.Text);
        }

        private void Hora_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox txt && !string.IsNullOrWhiteSpace(txt.Text))
            {
                if (!TimeSpan.TryParse(txt.Text, out var hora))
                {
                    MessageBox.Show("Informe uma hora válida no formato HH:mm",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    txt.Text = string.Empty;
                }
                else
                {
                    txt.Text = hora.ToString(@"hh\:mm");
                }
            }
        }

        private string GetEndAPI_formatado(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return string.Empty;

            var url = $"https://viacep.com.br/ws/{cep}/json/";




            using (var httpClient = new HttpClient())
            {
                var response = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
                using var json = JsonDocument.Parse(response);
                var root = json.RootElement;

                var logradouro = root.TryGetProperty("logradouro", out var l) ? l.GetString() : "";
                var bairro = root.TryGetProperty("bairro", out var b) ? b.GetString() : "";
                var localidade = root.TryGetProperty("localidade", out var c) ? c.GetString() : "";
                var uf = root.TryGetProperty("uf", out var u) ? u.GetString() : "";

                return $"{logradouro}, {bairro}, {localidade} - {uf}".Trim().Trim(',', '-');
            }
        }


    }
}
