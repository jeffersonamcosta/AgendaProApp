using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        }

        private void BtTelaFornecedores_Click(object sender, RoutedEventArgs e)
        {
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

        private async Task CarregarTiposEvento()
        {
            try
            {
                var json = await api.AgendaProGet("tipoevento/lista");
                if (json == null) return;

                var dict = new Dictionary<int, string>();
                dict[0] = "";

                foreach (var item in json.RootElement.EnumerateArray())
                {
                    dict[item.GetProperty("id").GetInt32()] = item.GetProperty("descricao").GetString();
                }

                Evento_cmbTipoEvento.ItemsSource = dict;
                Evento_cmbTipoEvento.DisplayMemberPath = "Value";
                Evento_cmbTipoEvento.SelectedValuePath = "Key";
                Evento_cmbTipoEvento.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar Tipos de Evento: {ex.Message}",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}
