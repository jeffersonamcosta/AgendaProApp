using System;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace AgendaProApp
{
    public partial class Principal
    {
        private Dictionary<int, string> _tiposparticipante   = new();

        public async void CarregarTiposParticipante()
        {
            var json = await api.AgendaProGet("participante/tipoparticipante");
         
            //var json = jsonTask.Result;
            _tiposparticipante.Clear();
            cmbTipoParticipanteId.Items.Clear();
            cmbTipoParticipanteId.Items.Add(new ComboBoxItem { Content = "Selecione", Tag = 0 });
            foreach (var item in json.RootElement.EnumerateArray())
            {
                var id = item.GetProperty("id").GetInt32();
                var nome = item.GetProperty("descricao").GetString() ?? "";
                _tiposparticipante[id] = nome;
                cmbTipoParticipanteId.Items.Add(new ComboBoxItem { Content = nome, Tag = id });
            }
            cmbTipoParticipanteId.SelectedIndex = 0;
        }
        private async void TELA_Participantes_btnIncluir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtNome.Text) || Convert.ToInt32(cmbTipoParticipanteId.SelectedValue ?? 0) == 0)
                {
                    MessageBox.Show("Nome e tipo são campos obrigatórios.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var rota = "participante/novo";
                var bodyJson = Tela_participantes_obterParticipanteJson();

                var result = await api.AgendaProPost(rota, bodyJson);

                MessageBox.Show("Participante incluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                Tela_Participantes_Limpar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao incluir participante: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Tela_Participantes_AtualizaLBParticipantes()
        {
            TELA_Participantes_LBCadastrado.Content = "";
            var tipoParticipanteqtd = "";
            foreach (var tipo in _tiposparticipante)
            {
                var count = 0;
                foreach (DataRowView row in TELA_Participantes_DTG.Items)
                {
                    if (Convert.ToInt32(row["TipoParticipanteId"]) == tipo.Key)
                    {
                        count++;
                    }
                }
                tipoParticipanteqtd += $" {count} {tipo.Value}, ";
            }
            TELA_Participantes_LBCadastrado.Content = $"{TELA_Participantes_DTG.Items.Count} Participantes Sendo: {tipoParticipanteqtd}";
            
        }

        private async void TELA_Participantes_btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            var body = Tela_participantes_obterParticipanteJson();
            var jsonTask = api.AgendaProPost("participante/pesquisar", body);
            var json = await jsonTask;

            var table = new DataTable();
           
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Nome", typeof(string));
            table.Columns.Add("Documento", typeof(string));
            table.Columns.Add("Telefone", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("TipoParticipanteId", typeof(int));
            table.Columns.Add("TipoParticipante", typeof(string));
            table.Columns.Add("Ativo", typeof(bool));

            foreach (var item in json.RootElement.EnumerateArray())
            {
                table.Rows.Add(
                    item.GetProperty("id").GetInt32(),
                    item.GetProperty("nome").GetString(),
                    item.GetProperty("documento").GetString(),
                    item.TryGetProperty("telefone", out var tel) ? tel.GetString() : "",
                    item.TryGetProperty("email", out var mail) ? mail.GetString() : "",
                    item.TryGetProperty("tipoParticipanteId", out var tipo) ? tipo.GetInt32() : 0,
                    _tiposparticipante[tipo.GetInt32()].ToString(),
                    item.TryGetProperty("ativo", out var ativo) && ativo.GetBoolean()
                );
            }

            TELA_Participantes_DTG.ItemsSource = table.DefaultView;
            // ocultar id, tipoParticipanteId e ativo 
            TELA_Participantes_DTG.Columns[0].Visibility = Visibility.Collapsed; 
            TELA_Participantes_DTG.Columns[5].Visibility = Visibility.Collapsed; 
            TELA_Participantes_DTG.Columns[7].Visibility = Visibility.Collapsed;
            Tela_Participantes_AtualizaLBParticipantes();

        }

        private void TELA_Participantes_DTG_seleciona(object sender, SelectionChangedEventArgs e)
        {
            if (TELA_Participantes_DTG.SelectedItem is DataRowView row)
            {
                txtNome.Text = row["Nome"].ToString();
                txtDocumento.Text = row["Documento"].ToString();
                txtTelefone.Text = row["Telefone"].ToString();
                txtEmail.Text = row["Email"].ToString();
                cmbTipoParticipanteId.SelectedValue = row["TipoParticipanteId"];
                chkAtivo.IsChecked = Convert.ToBoolean(row["Ativo"]);
            }
        }

        private void TELA_Participantes_btnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TELA_Participantes_DTG.SelectedItem is DataRowView row)
                {
                    var id = Convert.ToInt32(row["Id"]);
                    var rota = $"participante/atualiza/{id}";
                    var bodyJson = Tela_participantes_obterParticipanteJson();
                    bodyJson["id"] = id;
                    var result = api.AgendaProPut(rota, bodyJson);

                    MessageBox.Show("Participante atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Tela_Participantes_Limpar();
                }
                else
                {
                    MessageBox.Show("Selecione um participante na lista antes de atualizar.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar participante: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private JsonObject Tela_participantes_obterParticipanteJson()
        {
            var json = new JsonObject
            {
                ["Nome"] = txtNome.Text,
                ["Documento"] = txtDocumento.Text,
                ["Telefone"] = txtTelefone.Text,
                ["Email"] = txtEmail.Text,
                ["TipoParticipanteId"] = int.Parse(((ComboBoxItem)cmbTipoParticipanteId.SelectedItem).Tag.ToString()),
                ["Ativo"] = chkAtivo.IsChecked ?? false
            };

            return json;
        }

        private void Tela_Participantes_Limpar()
        {
            txtNome.Text = "";
            txtDocumento.Text = "";
            txtTelefone.Text = "";
            txtEmail.Text = "";
            cmbTipoParticipanteId.SelectedValue = 0;
            chkAtivo.IsChecked = true;
        }

        private void TELA_Participantes_btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            Tela_Participantes_Limpar();
            TELA_Participantes_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
        }
    }
}
