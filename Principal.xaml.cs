
using System.Data;
using System.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;


namespace AgendaProApp
{
    public partial class Principal : Window
    {
        private APIAgendaPro api;
        public Principal(APIAgendaPro api)
        {
            InitializeComponent();
            this.api = api;
            TELA_Fornecedor_Servicos_DTG_inciar();




        }

        private void TELA_Fornecedor_Servicos_DTG_inciar() {
            DataTable dt = new DataTable();
            dt.Columns.Add("Ativo", typeof(bool));
            dt.Columns.Add("Nome", typeof(string));
            dt.Columns.Add("Preco", typeof(decimal));            

            TELA_Fornecedor_Servicos_DTG.ItemsSource = dt.DefaultView;
            TELA_Fornecedor_Servicos_DTG.InitializingNewItem += TELA_Fornecedor_Servicos_DTG_InitializingNewItem;
        }
            

        private void BtTelaParticipantes_Click(object sender, RoutedEventArgs e)
        {
            TELA_Participantes.Visibility = Visibility.Visible;
            TELA_Fornecedores.Visibility = Visibility.Hidden;

        }

        private void BtTelaEventos_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtTelaFornecedores_Click(object sender, RoutedEventArgs e)
        {
            TELA_Participantes.Visibility = Visibility.Hidden;
            TELA_Fornecedores.Visibility = Visibility.Visible;
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
            var jsonTask = api.AgendaProPost("participante/pesquisar",body);
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
                ["TipoParticipanteId"] =  int.Parse(((ComboBoxItem)cmbTipoParticipanteId.SelectedItem).Tag.ToString()),
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
                var bodyJson = ObterFornecedorJson();
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
            var body = ObterFornecedorJson();
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
                    // Novo serviço
                    await api.AgendaProPost("servico/novo", body);
                }
                else
                {
                    // Serviço existente → adiciona Id no body
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
                    var bodyJson = ObterFornecedorJson();
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

        private JsonObject ObterFornecedorJson()
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

        private void TELA_Fornecedor_btnLimpar_Click(object sender, RoutedEventArgs e)
            
        {
            Tela_Fornecedor_Limpar();
        }
    }
}
