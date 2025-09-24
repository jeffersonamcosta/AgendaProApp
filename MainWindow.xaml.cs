using System.Net.Http;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AgendaProApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        private async void Login_click(object sender, RoutedEventArgs e)
        {
            string usuario = this.usuario.Text;
            string senha = this.senha.Password;
            Config.Load();

            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Informe usuário e senha.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var api = new APIAgendaPro(Config.Settings.BaseUrl);
                bool loginSuccess = await api.AgendaProLogin(usuario, senha);

                if (!loginSuccess)
                {
                    MessageBox.Show("Usuario ou senha incorreto", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else { 

                
                    
                var principal = new Principal(api);
                principal.Show();
                var telalogin = Application.Current.Windows[0];
                telalogin.Close();

            }


            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Erro na requisição: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}