
using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace AgendaProApp
{
    public partial class Principal : Window
    {
        private static readonly Regex regexHora = new Regex(@"^[0-9:]+$");
        private APIAgendaPro api;
        private Dictionary<int, JsonElement> _eventos = new();

        public Principal(APIAgendaPro api)
        {
            InitializeComponent();
            this.api = api;
            TELA_Fornecedor_Servicos_DTG_inciar();
        }

        private void TELA_Fornecedor_Servicos_DTG_inciar()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Ativo", typeof(bool));
            dt.Columns.Add("Nome", typeof(string));
            dt.Columns.Add("Preco", typeof(decimal));

            TELA_Fornecedor_Servicos_DTG.ItemsSource = dt.DefaultView;
            TELA_Fornecedor_Servicos_DTG.InitializingNewItem += TELA_Fornecedor_Servicos_DTG_InitializingNewItem;
        }


        private void MostrarTela(Grid telaVisivel)
        {

            TELA_Participantes.Visibility = Visibility.Hidden;
            TELA_Fornecedores.Visibility = Visibility.Hidden;
            TELA_Eventos.Visibility = Visibility.Hidden;
            telaVisivel.Visibility = Visibility.Visible;
        }

        // Eventos dos botões
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

        private void BtTelaRelatorios_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnIncluir_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPesquisar_Click(object sender, RoutedEventArgs e)
        {

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

                // Use await para chamar o método async corretamente
                var result = await api.AgendaProPost(rota, bodyJson);

                MessageBox.Show("Participante incluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                Tela_Participantes_Limpar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao incluir participante: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void TELA_Participantes_btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            //var jsonTask = api.AgendaProGet("participante/lista");
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
                    item.TryGetProperty("ativo", out var ativo) && ativo.GetBoolean()
                );
            }

            TELA_Participantes_DTG.ItemsSource = table.DefaultView;
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

        private async void TELA_Fornecedor_btnIncluir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Fornecedor_txtNome.Text) || string.IsNullOrWhiteSpace(Fornecedor_txtDocumento.Text))
                {
                    MessageBox.Show("Razão Social e CNPJ são campos obrigatórios.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var rota = "fornecedor/novo";
                var bodyJson = ObterFornecedorDaTelaEmJson();
                //var result = await api.AgendaProPost(rota, bodyJson);

                if (TELA_Fornecedor_Servicos_DTG.ItemsSource == null || (TELA_Fornecedor_Servicos_DTG.ItemsSource as DataView)?.Table.Rows.Count == 0)
                {
                    MessageBox.Show("Adicione pelo menos um serviço para o fornecedor.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    bodyJson["Servicos"] = new JsonArray();
                    foreach (DataRow row in (TELA_Fornecedor_Servicos_DTG.ItemsSource as DataView)?.Table.Rows)
                    {
                        var servicoBody = new JsonObject
                        {
                            ["Nome"] = row["Nome"].ToString(),
                            ["Preco"] = Convert.ToDecimal(row["Preco"]),
                            ["Ativo"] = Convert.ToBoolean(row["Ativo"])
                        };
                        (bodyJson["Servicos"] as JsonArray)?.Add(servicoBody);
                    }
                }
                await api.AgendaProPost(rota, bodyJson);

                MessageBox.Show("Fornecedor incluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                Tela_Fornecedor_Limpar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao incluir fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TELA_Fornecedor_Servicos_DTG_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            if (e.NewItem is DataRowView rowView)
            {
                rowView["Nome"] = "";
                rowView["Preco"] = 0;
                rowView["Ativo"] = true;
            }
        }

        private async void TELA_Fornecedor_btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            var body = ObterFornecedorDaTelaEmJson();
            var json = await api.AgendaProPost("fornecedor/pesquisar", body);

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Ativo", typeof(bool));
            table.Columns.Add("CNPJ", typeof(string));
            table.Columns.Add("RazaoSocial", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("Telefone", typeof(string));

            foreach (var item in json.RootElement.EnumerateArray())
            {
                table.Rows.Add(
                    item.GetProperty("id").GetInt32(),
                    item.TryGetProperty("ativo", out var ativo) && ativo.GetBoolean(),
                    item.GetProperty("cnpj").GetString(),
                    item.GetProperty("razaoSocial").GetString(),
                    item.TryGetProperty("email", out var mail) ? mail.GetString() : "",
                    item.TryGetProperty("telefone", out var tel) ? tel.GetString() : ""
                );
            }

            TELA_Fornecedor_DTG.ItemsSource = table.DefaultView;
        }

        private async void TELA_Fornecedor_DTG_seleciona(object sender, SelectionChangedEventArgs e)
        {
            if (TELA_Fornecedor_DTG.SelectedItem is DataRowView row)
            {
                Fornecedor_txtNome.Text = row["RazaoSocial"].ToString();
                Fornecedor_txtDocumento.Text = row["CNPJ"].ToString();
                Fornecedor_txtTelefone.Text = row["Telefone"].ToString();
                Fornecedor_txtEmail.Text = row["Email"].ToString();
                Fornecedor_chkAtivo.IsChecked = Convert.ToBoolean(row["Ativo"]);
                await TELA_Fornecedor_DTG_Servicos_Atualiza((int)row["id"]);
            }
        }

        private async Task TELA_Fornecedor_DTG_Servicos_Atualiza(int f_id)
        {
            var body = new JsonObject
            {
                ["FornecedorId"] = f_id,
                ["nome"] = ""
            };

            var json = await api.AgendaProPost("servico/pesquisar", body);
            if (json == null)
            {
                MessageBox.Show("Fornecedor sem servico", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Ativo", typeof(bool));
            table.Columns.Add("Nome", typeof(string));
            table.Columns.Add("Preco", typeof(decimal));


            foreach (var item in json.RootElement.EnumerateArray())
            {
                table.Rows.Add(
                    item.GetProperty("id").GetInt32(),
                    item.TryGetProperty("ativo", out var ativo) && ativo.GetBoolean(),
                    item.GetProperty("nome").GetString(),
                    item.GetProperty("preco").GetDecimal()
                );
            }

            TELA_Fornecedor_Servicos_DTG.ItemsSource = table.DefaultView;
        }

        private async Task Inserir_OU_Atualizar_Servicos(int fornecedorId, DataTable tabelaServicos)
        {
            foreach (DataRow row in tabelaServicos.Rows)
            {
                // Monta o JSON do serviço
                var body = new JsonObject
                {
                    ["FornecedorId"] = fornecedorId,
                    ["Nome"] = row["Nome"].ToString(),
                    ["Preco"] = Convert.ToDecimal(row["Preco"]),
                    ["Ativo"] = Convert.ToBoolean(row["Ativo"])
                };

                if (row.IsNull("Id") || (int)row["Id"] == 0)
                {
                   
                    await api.AgendaProPost("servico/novo", body);
                }
                else
                {
            
                    body["Id"] = (int)row["Id"];

                    await api.AgendaProPut($"servico/atualiza/{body["Id"]}", body);
                }
            }
        }

        private async void TELA_Fornecedor_btnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TELA_Fornecedor_DTG.SelectedItem is DataRowView row)
                {
                    var id = Convert.ToInt32(row["Id"]);
                    var rota = $"fornecedor/atualiza/{id}";
                    var bodyJson = ObterFornecedorDaTelaEmJson();
                    bodyJson["id"] = id;

                    var result = await api.AgendaProPut(rota, bodyJson);
                    await Inserir_OU_Atualizar_Servicos(id, (TELA_Fornecedor_Servicos_DTG.ItemsSource as DataView)?.Table);

                    MessageBox.Show("Fornecedor atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Tela_Fornecedor_Limpar();
                }
                else
                {
                    MessageBox.Show("Selecione um fornecedor na lista antes de atualizar.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao atualizar fornecedor: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private JsonObject ObterFornecedorDaTelaEmJson()
        {
            var json = new JsonObject
            {
                ["RazaoSocial"] = Fornecedor_txtNome.Text,
                ["CNPJ"] = Fornecedor_txtDocumento.Text,
                ["Telefone"] = Fornecedor_txtTelefone.Text,
                ["Email"] = Fornecedor_txtEmail.Text,
                ["Ativo"] = Fornecedor_chkAtivo.IsChecked ?? false
            };
            return json;
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
                decimal.TryParse(Evento_txtOrcamento.Text, out var orcamento))
            {
                json["orcamentoMaximo"] = orcamento;           }

           
       


            if (Evento_cmbTipoEvento.SelectedIndex is int tipoEventoId)
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

        private string ObterStringDataTime(string date, string time ) {
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

        private void Tela_Fornecedor_Limpar()
        {
            Fornecedor_txtNome.Text = "";
            Fornecedor_txtDocumento.Text = "";
            Fornecedor_txtTelefone.Text = "";
            Fornecedor_txtEmail.Text = "";
            Fornecedor_chkAtivo.IsChecked = true;
            TELA_Fornecedor_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
            TELA_Fornecedor_Servicos_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
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


        private void TELA_Fornecedor_btnLimpar_Click(object sender, RoutedEventArgs e)

        {
            Tela_Fornecedor_Limpar();
        }

        private async void TELA_Eventos_btnIncluir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Evento_txtNome.Text) || string.IsNullOrWhiteSpace(Evento_dpInicio.Text) )
            {
                MessageBox.Show("Campos obrigatórios Pendentes.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var bodyJson = GetTelaEventoJson();
            var rota = "eventos/novo";
            var result = await api.AgendaProPost(rota, bodyJson);
            MessageBox.Show("Evento incluído com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            Tela_Eventos_Limpar();
        }

        private async void TELA_Eventos_btnAtualizar_Click(object sender, RoutedEventArgs e)
        {

                var bodyJson = GetTelaEventoJson();
                var rota = $"eventos/atualiza/{bodyJson["id"]}";
                var result = await api.AgendaProPut(rota, bodyJson);
                

        }



        private void TELA_Eventos_btnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var body = GetTelaEventoJson();
                var jsonTask = api.AgendaProPost("eventos/pesquisar", body);
                var json = jsonTask.Result;
                if (json == null) return;               
               // MessageBox.Show(JsonSerializer.Serialize(json.RootElement, new JsonSerializerOptions { WriteIndented = true }));
                
                
                var table_eventos = new DataTable();
                table_eventos.Columns.Add("Id", typeof(int));
                table_eventos.Columns.Add("Nome", typeof(string));
                table_eventos.Columns.Add("DataInicio", typeof(DateTime));
                table_eventos.Columns.Add("DataFim", typeof(DateTime));
               // table_eventos.Columns.Add("TipoEvento", typeof(string));
                table_eventos.Columns.Add("Ativo", typeof(bool));
                table_eventos.Columns.Add(
                    "Orcamento", typeof(decimal));
                table_eventos.Columns.Add(
                    "CapacidadeMaxima", typeof(int));
                _eventos.Clear();
                foreach (var item in json.RootElement.EnumerateArray())
                    {

                    int id = item.GetProperty("id").GetInt32();
                    _eventos[id] = item; 

                    table_eventos.Rows.Add(
                        item.GetProperty("id").GetInt32(),
                        item.GetProperty("nome").GetString(),
                        item.GetProperty("dataInicio").GetDateTime(),
                        item.GetProperty("dataFim").GetDateTime(),
                        //item.GetProperty("tipoEventoId").GetString(),
                        item.TryGetProperty("ativo", out var ativo) && ativo.GetBoolean(),
                        item.TryGetProperty("orcamentoMaximo", out var orcamento) ? orcamento.GetDecimal() : 0,
                        item.TryGetProperty("capacidadeMaxima", out var capacidade) ? capacidade.GetInt32() : 0
                    );
                }
                TELA_Eventos_DTG.ItemsSource = table_eventos.DefaultView;
                 


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
            Evento_dpInicio.Text = "";
            Evento_dpFim.Text = "";
            Evento_chkAtivo.IsChecked = true;
            TELA_Eventos_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
            TELA_Eventos_Participantes_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
            TELA_Eventos_Servicos_DTG.ClearValue(ItemsControl.ItemsSourceProperty);
            Evento_txtCapacidadeMaxima.Text = "";
            Evento_txtOrcamento.Text = "";
            Evento_txtOrcamentousado.Text = "";           
            Evento_txtCEP.Text="";
            Evento_txtEndereco.Text="";
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

            
            table.Columns["id"].ReadOnly = true;
            table.Columns["Nome"].ReadOnly = true;
            table.Columns["Convidado"].ReadOnly = false;

            foreach (var item in json.RootElement.EnumerateArray())
            {
                table.Rows.Add(
                    item.GetProperty("id").GetInt32(),
                    item.GetProperty("nome").GetString(),
                    idSelecionados != null && idSelecionados.Contains(item.GetProperty("id").GetInt32())// verifica se o id está na lista de convidado e adiciona na tela
                );
            }
            (table.DefaultView).Sort = "Convidado DESC";
            TELA_Eventos_Participantes_DTG.ItemsSource = table.DefaultView;

        }




        private void Hora_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
           
            e.Handled = !regexHora.IsMatch(e.Text);
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
            table.Columns["id"].ReadOnly = true;
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
                    idSelecionados != null && idSelecionados.Contains(item.GetProperty("id").GetInt32()));// verifica se o id está na lista de servicos e adiciona na tela

            }
            (table.DefaultView).Sort = "Selecionado DESC";
            TELA_Eventos_Servicos_DTG.ItemsSource = table.DefaultView;
        }

        private double ObterSomaServicosSelecionados()
        {
            var x = new double();
            x = 0;

            if (TELA_Eventos_Servicos_DTG.ItemsSource is DataView view)
            {
                foreach (DataRowView row in view)
                {
                    bool marcado = row["Selecionado"] != DBNull.Value && (bool)row["Selecionado"];
                    if (marcado)
                    {
                        int id = (int)row["Id"];
                        x += Convert.ToDouble(row["preco"]); ;
                    }
                }
            }

            return x;
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
                SetTelaEventoJson(_eventos[(int)row["id"]]);
            } 
        }

        private void SetTelaEventoJson(JsonElement json)
        {
            try
            {
                
                // Texto simples
                if (json.TryGetProperty("nome", out var nome))
                    Evento_txtNome.Text = nome.GetString() ?? "";

                if (json.TryGetProperty("cep", out var cep))
                    Evento_txtCEP.Text = cep.GetString() ?? "";

                if (json.TryGetProperty("endereco", out var endereco))
                    Evento_txtEndereco.Text = endereco.GetString() ?? "";

                if (json.TryGetProperty("observacoes", out var obs))
                    Evento_txtObservacoes.Text = obs.GetString() ?? "";

                // Datas e horas
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

                // Valores numéricos
                if (json.TryGetProperty("capacidadeMaxima", out var capacidade))
                    Evento_txtCapacidadeMaxima.Text = capacidade.GetInt32().ToString();

                if (json.TryGetProperty("orcamentoMaximo", out var orcamento))
                    Evento_txtOrcamento.Text = orcamento.GetDecimal().ToString("F2");

                // CheckBox
                if (json.TryGetProperty("ativo", out var ativo))
                    Evento_chkAtivo.IsChecked = ativo.GetBoolean();

                // ComboBox TipoEvento
                if (json.TryGetProperty("tipoEventoId", out var tipoId))
                {
                    int id = tipoId.GetInt32();
                    // seleciona pelo SelectedValue ou encontra no ItemsSource
                    foreach (var item in Evento_cmbTipoEvento.Items)
                    {
                        if (item is KeyValuePair<int, string> kv && kv.Key == id)
                        {
                            Evento_cmbTipoEvento.SelectedItem = kv;
                            break;
                        }
                        // caso ainda use TipoEventoItem:
                        // if (item is TipoEventoItem te && te.Id == id) { Evento_cmbTipoEvento.SelectedItem = te; break; }
                    }
                }

                if (json.TryGetProperty("participantesIds", out var participantesIds)){
                    var listaParticipantes = participantesIds.EnumerateArray()
                                         .Select(p => p.GetInt32())
                                         .ToList();

                    TELA_Eventos_Participantes_DTG_Preencher(listaParticipantes);
                }

                if (json.TryGetProperty("servicosIds", out var servicosIds))
                {
                    var listaServicos = servicosIds.EnumerateArray()
                                         .Select(s => s.GetInt32())
                                         .ToList();
                    TELA_Eventos_DTG_Servicos_Preencher(listaServicos);
                   
                }
                TELA_Eventos_txtOrcamentousado_update();


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao preencher tela: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
