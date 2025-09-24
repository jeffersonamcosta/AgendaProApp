using System;
using System.Data;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;

namespace AgendaProApp
{
    public partial class Principal
    {
        private void TELA_Fornecedor_Servicos_DTG_inciar()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Ativo", typeof(bool));
            dt.Columns.Add("Nome", typeof(string));
            dt.Columns.Add("Preco", typeof(decimal));

            TELA_Fornecedor_Servicos_DTG.ItemsSource = dt.DefaultView;
            TELA_Fornecedor_Servicos_DTG.InitializingNewItem += TELA_Fornecedor_Servicos_DTG_InitializingNewItem;
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
