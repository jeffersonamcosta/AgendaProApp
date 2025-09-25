using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace AgendaProApp
{
    public partial class Principal
    {
        private Dictionary<int, JsonElement> _eventos = new();
        private async void TELA_Eventos_btnIncluir_Click(object sender, RoutedEventArgs e)
        {
            if (CamposValidos())
            {

                var bodyJson = GetTelaEventoJson();

                if (bodyJson.ContainsKey("id"))
                {
                    bodyJson.Remove("id");
                }
                var rota = "eventos/novo";
                var result = await api.AgendaProPost(rota, bodyJson);
                MessageBox.Show("Evento incluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                Tela_Eventos_Limpar();
            }
        }

        private async void TELA_Eventos_btnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            if (CamposValidos())
            {

                var bodyJson = GetTelaEventoJson();
                var id = bodyJson["id"].GetValue<int>();
                var rota = $"eventos/atualiza/{id}";
                var result = await api.AgendaProPut(rota, bodyJson);
                Tela_Eventos_Limpar();
            }


        }

        private void TELA_Eventos_btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var body = GetTelaEventoJson();
                var jsonTask = api.AgendaProPost("eventos/pesquisar", body);
                var json = jsonTask.Result;
                if (json == null) return;

                var table_eventos = new DataTable();
                table_eventos.Columns.Add("Id", typeof(int));
                table_eventos.Columns.Add("Nome", typeof(string));
                table_eventos.Columns.Add("DataInicio", typeof(DateTime));
                table_eventos.Columns.Add("DataFim", typeof(DateTime));
                table_eventos.Columns.Add("Orcamento", typeof(decimal));
                table_eventos.Columns.Add("CapacidadeMaxima", typeof(int));
                table_eventos.Columns.Add("Endereco", typeof(string));
                table_eventos.Columns.Add("OBS", typeof(string));
                table_eventos.Columns.Add("Ativo", typeof(bool));

                _eventos.Clear();
                foreach (var item in json.RootElement.EnumerateArray())
                {
                    int id = item.GetProperty("id").GetInt32();
                    _eventos[id] = item;

                    table_eventos.Rows.Add(
                        id,
                        item.GetProperty("nome").GetString(),
                        item.GetProperty("dataInicio").GetDateTime(),
                        item.GetProperty("dataFim").GetDateTime(),
                        item.TryGetProperty("orcamentoMaximo", out var orcamento) ? orcamento.GetDecimal() : 0,
                        item.TryGetProperty("capacidadeMaxima", out var capacidade) ? capacidade.GetInt32() : 0,
                        item.TryGetProperty("endereco", out var endereco) ? endereco.GetString() : "",
                        item.TryGetProperty("observacoes", out var obs) ? obs.GetString() : "",
                        item.TryGetProperty("ativo", out var ativo) && ativo.GetBoolean()
                    );
                }

                TELA_Eventos_DTG.ItemsSource = table_eventos.DefaultView;
                TELA_Eventos_DTG.Columns[0].Visibility = Visibility.Collapsed;
                TELA_Eventos_DTG.Columns[7].Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao pesquisar evento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TELA_Eventos_btnLimpar_Click(object sender, RoutedEventArgs e)
        {
            Tela_Eventos_Limpar();
        }

        private void Tela_Eventos_Limpar()
        {
            Evento_txtNome.Text = "";
            Evento_txtObservacoes.Text = "";
            Evento_dpInicio.Text = "";
            Evento_txtHRFim.Text = "";
            Evento_dpFim.Text = "";
            Evento_chkAtivo.IsChecked = true;
            TELA_Eventos_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
            TELA_Eventos_Participantes_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
            TELA_Eventos_Servicos_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
            Evento_txtCapacidadeMaxima.Text = "";
            Evento_txtCapacidadeUsado.Text = "";
            Evento_txtOrcamento.Text = "";
            Evento_txtOrcamentousado.Text = "";
            Evento_txtCEP.Text = "";
            Evento_txtEndereco.Text = "";
            _eventos.Clear();
        }

        private async void TELA_Eventos_btnListarParticipantes_Click(object sender, RoutedEventArgs e)
        {
            TELA_Eventos_Participantes_DTG_Preencher();
        }

        private async void TELA_Eventos_Participantes_DTG_Preencher(List<int> idSelecionados = null)
        {
            var body = new JsonObject
            {
                ["nome"] = "",
                ["documento"] = "",
                ["tipoParticipanteId"] = 0,
                ["ativo"] = true
            };

            var rota = "participante/pesquisar";
            var json = await api.AgendaProPost(rota, body);
            if (json == null) return;

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Nome", typeof(string));
            table.Columns.Add("Convidado", typeof(bool));

            table.Columns["Id"].ReadOnly = true;
            table.Columns["Nome"].ReadOnly = true;
            table.Columns["Convidado"].ReadOnly = false;

            foreach (var item in json.RootElement.EnumerateArray())
            {
                table.Rows.Add(
                    item.GetProperty("id").GetInt32(),
                    item.GetProperty("nome").GetString(),
                    idSelecionados != null && idSelecionados.Contains(item.GetProperty("id").GetInt32())
                );
            }

            (table.DefaultView).Sort = "Convidado DESC";
            TELA_Eventos_Participantes_DTG.ItemsSource = table.DefaultView;
            TELA_Eventos_Participantes_DTG.Columns[0].Visibility = Visibility.Collapsed;
        }

        private void TELA_Eventos_btnListarServicos_Click(object sender, RoutedEventArgs e)
        {
            TELA_Eventos_DTG_Servicos_Preencher();
        }

        private void TELA_Eventos_DTG_Servicos_Preencher(List<int> idSelecionados = null)
        {
            var json = api.AgendaProGet("servico/listaValidos");
            if (json == null) return;

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Fornecedor", typeof(string));
            table.Columns.Add("Servico", typeof(string));
            table.Columns.Add("Preco", typeof(decimal));
            table.Columns.Add("Selecionado", typeof(bool));

            table.Columns["Selecionado"].ReadOnly = false;
            table.Columns["Id"].ReadOnly = true;
            table.Columns["Fornecedor"].ReadOnly = true;
            table.Columns["Servico"].ReadOnly = true;
            table.Columns["Preco"].ReadOnly = true;

            foreach (var item in json.Result.RootElement.EnumerateArray())
            {
                table.Rows.Add(
                    item.GetProperty("id").GetInt32(),
                    item.GetProperty("fornecedornome").GetString(),
                    item.GetProperty("nome").GetString(),
                    item.GetProperty("preco").GetDecimal(),
                    idSelecionados != null && idSelecionados.Contains(item.GetProperty("id").GetInt32())
                );
            }

            (table.DefaultView).Sort = "Selecionado DESC";
            TELA_Eventos_Servicos_DTG.ItemsSource = table.DefaultView;
            TELA_Eventos_Servicos_DTG.Columns[0].Visibility = Visibility.Collapsed;
        }

        private double ObterSomaServicosSelecionados()
        {
            double total = 0;

            if (TELA_Eventos_Servicos_DTG.ItemsSource is DataView view)
            {
                foreach (DataRowView row in view)
                {
                    bool marcado = row["Selecionado"] != DBNull.Value && (bool)row["Selecionado"];
                    if (marcado)
                    {
                        total += Convert.ToDouble(row["Preco"]);
                    }
                }
            }

            return total;
        }

        private void TELA_Eventos_Servicos_DTG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            TELA_Eventos_txtOrcamentousado_update();
        }


        private void TELA_Eventos_txtOrcamentousado_update()
        {
            Evento_txtOrcamentousado.Text = "";
            Evento_txtOrcamentousado.Text = ObterSomaServicosSelecionados().ToString();
        }

        private void TELA_Eventos_DTG_seleciona(object sender, SelectedCellsChangedEventArgs e)
        {
            if (TELA_Eventos_DTG.SelectedItem is DataRowView row)
            {
                SetTelaEventoJson(_eventos[(int)row["Id"]]);
            }
        }

        private void SetTelaEventoJson(JsonElement json)
        {
            try
            {
                if (json.TryGetProperty("nome", out var nome))
                    Evento_txtNome.Text = nome.GetString() ?? "";

                if (json.TryGetProperty("cep", out var cep))
                    Evento_txtCEP.Text = cep.GetString() ?? "";

                if (json.TryGetProperty("endereco", out var endereco))
                    Evento_txtEndereco.Text = endereco.GetString() ?? "";

                if (json.TryGetProperty("observacoes", out var obs))
                    Evento_txtObservacoes.Text = obs.GetString() ?? "";

                if (json.TryGetProperty("dataInicio", out var dtInicio))
                {
                    var dt = dtInicio.GetDateTime();
                    Evento_dpInicio.SelectedDate = dt.Date;
                    Evento_txtHRInicio.Text = dt.ToString("HH:mm");
                }

                if (json.TryGetProperty("dataFim", out var dtFim))
                {
                    var dt = dtFim.GetDateTime();
                    Evento_dpFim.SelectedDate = dt.Date;
                    Evento_txtHRFim.Text = dt.ToString("HH:mm");
                }

                if (json.TryGetProperty("capacidadeMaxima", out var capacidade))
                    Evento_txtCapacidadeMaxima.Text = capacidade.GetInt32().ToString();

                if (json.TryGetProperty("orcamentoMaximo", out var orcamento))
                    Evento_txtOrcamento.Text = orcamento.GetDecimal().ToString("F2");

                if (json.TryGetProperty("ativo", out var ativo))
                    Evento_chkAtivo.IsChecked = ativo.GetBoolean();

                if (json.TryGetProperty("tipoEventoId", out var tipoId))
                {
                    int id = tipoId.GetInt32();
                    foreach (var item in Evento_cmbTipoEvento.Items)
                    {
                        if (item is KeyValuePair<int, string> kv && kv.Key == id)
                        {
                            Evento_cmbTipoEvento.SelectedItem = kv;
                            break;
                        }
                    }
                }

                if (json.TryGetProperty("participantesIds", out var participantesIds))
                {
                    var listaParticipantes = participantesIds.EnumerateArray().Select(p => p.GetInt32()).ToList();
                    TELA_Eventos_Participantes_DTG_Preencher(listaParticipantes);
                }

                if (json.TryGetProperty("servicosIds", out var servicosIds))
                {
                    var listaServicos = servicosIds.EnumerateArray().Select(s => s.GetInt32()).ToList();
                    TELA_Eventos_DTG_Servicos_Preencher(listaServicos);
                }

                TELA_Eventos_txtOrcamentousado_update();
                Tela_Eventos_txtCapacidadeUsada_update();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao preencher tela: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private JsonObject GetTelaEventoJson()
        {
            var json = new JsonObject();

            if (TELA_Eventos_DTG.SelectedItem is DataRowView ev_dtg_row && ev_dtg_row["Id"] != DBNull.Value)
            {
                json["id"] = (int)ev_dtg_row["Id"];
            }

            if (!string.IsNullOrWhiteSpace(Evento_txtNome.Text))
                json["nome"] = Evento_txtNome.Text;

            if (!string.IsNullOrWhiteSpace(Evento_txtObservacoes.Text))
                json["observacoes"] = Evento_txtObservacoes.Text;

            var dataInicio = ObterStringDataTime(Evento_dpInicio.Text, Evento_txtHRInicio.Text);
            if (!string.IsNullOrWhiteSpace(dataInicio))
                json["dataInicio"] = dataInicio;

            var dataFim = ObterStringDataTime(Evento_dpFim.Text, Evento_txtHRFim.Text);
            if (!string.IsNullOrWhiteSpace(dataFim))
                json["dataFim"] = dataFim;

            if (!string.IsNullOrWhiteSpace(Evento_txtCEP.Text))
                json["cep"] = Evento_txtCEP.Text;

            if (!string.IsNullOrWhiteSpace(Evento_txtEndereco.Text))
                json["endereco"] = Evento_txtEndereco.Text;

            if (!string.IsNullOrWhiteSpace(Evento_txtCapacidadeMaxima.Text) &&
                int.TryParse(Evento_txtCapacidadeMaxima.Text, out var capacidade))
            {
                json["capacidadeMaxima"] = capacidade;
            }

            if (!string.IsNullOrWhiteSpace(Evento_txtOrcamento.Text) &&
                decimal.TryParse(Evento_txtOrcamentousado.Text, out var orcamento))
            {
                json["orcamentoMaximo"] = orcamento;
            }

            if (Evento_cmbTipoEvento.SelectedValue is int tipoEventoId)
                json["tipoEventoId"] = tipoEventoId;

            if (Evento_chkAtivo.IsChecked.HasValue)
                json["ativo"] = Evento_chkAtivo.IsChecked.Value;

            var participantesids = new JsonArray();
            if (TELA_Eventos_Participantes_DTG.ItemsSource is DataView view)
            {
                foreach (DataRowView row in view)
                {
                    bool marcado = row["Convidado"] != DBNull.Value && (bool)row["Convidado"];
                    if (marcado)
                    {
                        int id = (int)row["Id"];
                        participantesids.Add(id);
                    }
                }
            }
            json["participantesIds"] = participantesids;

            var servicosids = new JsonArray();
            if (TELA_Eventos_Servicos_DTG.ItemsSource is DataView view2)
            {
                foreach (DataRowView row in view2)
                {
                    bool marcado = row["Selecionado"] != DBNull.Value && (bool)row["Selecionado"];
                    if (marcado)
                    {
                        int id = (int)row["Id"];
                        servicosids.Add(id);
                    }
                }
            }
            json["servicosIds"] = servicosids;

            return json;
        }

        private void Evento_txtCEP_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Evento_txtCEP.Text.Length == 8 && !string.IsNullOrWhiteSpace(Evento_txtCEP.Text))
            {
                Evento_txtEndereco.Text = GetEndAPI_formatado(Evento_txtCEP.Text);
            }

        }
        private void TELA_Eventos_Participantes_DTG_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            Tela_Eventos_txtCapacidadeUsada_update();
        }
        private void Tela_Eventos_txtCapacidadeUsada_update()
        {
            int total = 0;

            if (TELA_Eventos_Participantes_DTG.ItemsSource is DataView view) { foreach (DataRowView row in view) { bool marcado = row["Convidado"] != DBNull.Value && (bool)row["Convidado"]; if (marcado) { total++; } } }
            Evento_txtCapacidadeUsado.Text = total.ToString();

        }

        private Boolean OrcamentoValido()
        {
            if (string.IsNullOrWhiteSpace(Evento_txtOrcamento.Text) || string.IsNullOrWhiteSpace(Evento_txtOrcamentousado.Text))
            {
                return false;
            }
            if (decimal.TryParse(Evento_txtOrcamento.Text, out var orcamentoMaximo) &&
                decimal.TryParse(Evento_txtOrcamentousado.Text, out var orcamentoUsado))
            {
                if (orcamentoUsado <= orcamentoMaximo)
                {
                    return true;
                } ;
            }
            return false;
        }
        private Boolean lotacaoValida()
        {
            if (string.IsNullOrWhiteSpace(Evento_txtCapacidadeMaxima.Text) || string.IsNullOrWhiteSpace(Evento_txtCapacidadeUsado.Text))
            {
                return false;
            }
            if (int.TryParse(Evento_txtCapacidadeMaxima.Text, out var capacidadeMaxima) &&
                int.TryParse(Evento_txtCapacidadeUsado.Text, out var capacidadeUsada))
            {
               if (capacidadeUsada <= capacidadeMaxima){
                    return true;
                }
            }
            return false;
        }

        private Boolean CamposValidos()
        {
            if (string.IsNullOrWhiteSpace(Evento_txtNome.Text) || string.IsNullOrWhiteSpace(Evento_dpInicio.Text) || Evento_cmbTipoEvento.SelectedValue is not int)
            {
                MessageBox.Show("Campos obrigatórios Pendentes.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!OrcamentoValido())
            {
                MessageBox.Show("Não se pode ultrapassar o oirçamento", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!lotacaoValida())
            {
                MessageBox.Show("Capacidade máxima não pode ser menor que a capacidade usada.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void Evento_btnNovoTipoEvento_Click(object sender, RoutedEventArgs e)
        {if (string.IsNullOrWhiteSpace(Evento_NovoTipoEvento.Text))
            {
                MessageBox.Show("Descrição do tipo de evento não pode ser vazia.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var body = new JsonObject
            {
                ["descricao"] = Evento_NovoTipoEvento.Text,
            
            };
            var rota = "tipoevento/novo";
            var result = api.AgendaProPost(rota, body);
            var idnovo = result.Result.RootElement.GetProperty("id").GetInt32();
            CarregarTiposEvento();
            Evento_cmbTipoEvento.SelectedValue = idnovo;
            Evento_STPNovoTipoEvento.Visibility = Visibility.Hidden;

        }

        private void Evento_cmbTipoEvento_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Evento_cmbTipoEvento.SelectedIndex == 0 )
            {
                Evento_STPNovoTipoEvento.Visibility = Visibility.Visible;

            }
            else {                 Evento_STPNovoTipoEvento.Visibility = Visibility.Hidden; }

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

    }
}
