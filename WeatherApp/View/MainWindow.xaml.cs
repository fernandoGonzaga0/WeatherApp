using System.Windows;
using System.Text.Json;
using System.Net.Http;
using WeatherApp.Models;

/*

Objetivo: implementar a janela principal da aplicação WPF que consulta a API do OpenWeatherMap e exibe nome da cidade, temperatura e descrição do tempo.

O que cada parte faz:

    • Usings: importam bibliotecas para HTTP, JSON e UI.

    • Campos da classe:

        + _httpClient: realiza requisições HTTP de forma eficiente e reutilizável.
        + apiKey: armazena a chave da API (para desenvolvimento; em produção use variáveis de ambiente/secret store).
        + _jsonOptions: configura desserialização para não diferenciar maiúsculas/minúsculas nos nomes das propriedades JSON.

    • Construtor: chama InitializeComponent() para montar a interface XAML.

    • Button_Click(object, RoutedEventArgs):

        + Lê a cidade do SearchBox.
        + Monta a URL de requisição usando a cidade e a chave.
        + Faz a requisição HTTP e verifica o status.
        + Lê e desserializa o JSON para WeatherResponse.
        + Faz checagens defensivas para evitar NullReferenceException.
        + Atualiza os controles de UI (CityResult, TempResult, DescriptionResult).
        + Trata exceções inesperadas e loga para depuração.
        + Modelos (WeatherResponse, MainInfo, WeatherInfo): representam a estrutura do JSON retornado pela API.

*/

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        // cliente HTTP reutilizável para fazer requisições à API
        private readonly HttpClient _httpClient = new HttpClient();

        // chave da API 
        private readonly string apiKey = "af8bee2cf657f1295ededfda82c02255";

        // construtor que inicializa a janela
        public MainWindow()
        {
            InitializeComponent();
        }

        // manipulador de clique do botão que busca o tempo para a cidade informada
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            // obtendo o nome da cidade a partir do TextBox
            string city = SearchBox.Text;

            // monta a URL da API usando a cidade e a chave da API
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=pt_br";

            try
            {
                // validando se o SearchBox não está vazio
                if (string.IsNullOrWhiteSpace(SearchBox.Text))
                {
                     // caso esteja vazio, exibe mensagem para inserção de alguma cidade
                     MessageBox.Show("Por favor, insira o nome de uma cidade.");
                    SearchBox.Focus();
                    return;
                }

                // envia a requisição GET para a API
                var httpResponse = await _httpClient.GetAsync(url);

                // se a resposta não for bem-sucedida, exibe mensagem de erro na UI
                if (!httpResponse.IsSuccessStatusCode)
                {
                    CityResult.Text = "Cidade não encontrada!";
                    TempResult.Text = "Temperatura não encontrada!";
                    DescriptionResult.Text = "Detalhes não encontrados!";
                    WindResult.Text = "Informações sobre vento não encontradas!";
                    return;
                }

                // lê o conteúdo da resposta como string JSON
                var response = await httpResponse.Content.ReadAsStringAsync();

                // configura opções de desserialização para ignorar diferenças de maiúsculas/minúsculas
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // desserializa o JSON para o objeto WeatherResponse
                var weather = JsonSerializer.Deserialize<WeatherResponse>(response, options);

                // verificação defensiva para garantir que os dados esperados existem antes de acessá-los
                if (weather?.Main == null || weather.Weather == null || weather.Weather.Length == 0)
                {
                    CityResult.Text = "Dados incompletos!";
                    TempResult.Text = "";
                    DescriptionResult.Text = "";
                    WindResult.Text = "";
                }

                // Atualiza a UI com os valores desserializados
                CityResult.Text = weather.Name ?? "-";
                TempResult.Text = $"{weather.Main.Temp} °C";
                DescriptionResult.Text = weather.Weather[0].Description ?? "-";
                WindResult.Text = $"{weather.Wind.Speed} m/s";
            }
            catch (Exception ex)
            {
                // em um cenário onde aconteça um erro, mostra na UI essa mensagem genérica
                CityResult.Text = "Erro inesperado!";
                TempResult.Text = "";
                DescriptionResult.Text = "";
                WindResult.Text = "";

                // para depuração local, logando ex.Message/stacktrace em log file ou Debug.WriteLine
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}