using System;
using System.Windows;
using System.Text.Json;
using System.Net.Http;
using WeatherApp.Models;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        // CAMPOS E CONSTANTES

            // cliente HTTP reutilizável para fazer requisições à API
            private readonly HttpClient _httpClient = new HttpClient();

            // chave da API 
            private readonly string apiKey = "af8bee2cf657f1295ededfda82c02255";
        
        // CONSTRUTORES

            // construtor que inicializa a janela
            public MainWindow()
            {
                InitializeComponent();
            }

        // MANIPULADORES DE EVENTOS

            // manipulador de clique do botão que busca o tempo para a cidade informada
            private async void Button_Click(object sender, RoutedEventArgs e)
            {
                // CAMPOS E CONSTANTES

                    // obtendo o nome da cidade a partir do TextBox
                    string city = SearchBox.Text;

                    // monta a URL da API usando a cidade e a chave da API
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric&lang=pt_br";

                try
                {
                    // CAMPOS E CONSTANTES

                        // VARIÁVEIS PARA CONEXÃO COM API E TRATATIVAS DE DADOS

                            // envia a requisição GET para a API
                            var httpResponse = await _httpClient.GetAsync(url);

                            // lê o conteúdo da resposta como string JSON
                            var response = await httpResponse.Content.ReadAsStringAsync();

                            // configura opções de desserialização para ignorar diferenças de maiúsculas/minúsculas
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };   
                
                            // desserializa o JSON para o objeto WeatherResponse
                            var weather = JsonSerializer.Deserialize<WeatherResponse>(response, options);
                            // Atualiza a UI com os valores desserializados

                        // COMPONENTES PARA ATUALIZAÇÃO DA UI COM OS VALORES DESSERIALIZADOS

                            // nome da cidade
                            CityResult.Text = weather.Name ?? "-";
                            
                            // componentes para conversão de horário
                            double timezoneInSeconds = weather.Timezone; // timezone chega da API em segundos
                            DateTime utcNow = DateTime.UtcNow; // criando e definindo um DateTime atual
                            DateTime localTime = DateTime.UtcNow.AddSeconds(timezoneInSeconds);
                            TimezoneResult.Text = localTime.ToString("HH:mm");

                            // temperatura
                            TempResult.Text = $"{weather.Main.Temp} °C";

                            // sensação térmica
                            FeelsLikeResult.Text = $"{weather.Main.FeelsLike} °C";

                            // humidade
                            HumidityResult.Text = $"{weather.Main.Humidity} %";

                            // descrição do tempo
                            DescriptionResult.Text = weather.Weather[0].Description ?? "-";

                            // velocidade do vento
                            WindResult.Text = $"{weather.Wind.Speed} m/s";

                        // COMPONENTES PARA INSERÇÃO DE IMAGENS CONFORME RETORNO DA API (description)

                            // obtendo o retorno em texto da API da classe main no objeto description
                            string? description = weather.Weather[0].Description?.ToLower();

                            // convertendo o horário local da cidade para validar se é dia (entre 06:00 e 18:59) ou noite (entre 19:00 e 05:59)
                            int timezoneDayOrNight = int.Parse(TimezoneResult.Text.Substring(0, 2));

                            // boolean para verificar se é dia ou não
                            bool isDay = timezoneDayOrNight is >= 6 and <= 18;

                            // caminho relativo para a imagem exibida
                            string resourcePath;
                                
                    // CONDICIONAIS

                        // validando se o SearchBox não está vazio
                        if (string.IsNullOrWhiteSpace(SearchBox.Text))
                        {
                             // caso esteja vazio, exibe mensagem para inserção de alguma cidade
                             MessageBox.Show("Por favor, insira o nome de uma cidade.");
                            SearchBox.Focus();
                            return;
                        }

                        // se a resposta não for bem-sucedida, exibe mensagem de erro na UI
                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            CityResult.Text = "Cidade não encontrada!";
                            TempResult.Text = "Temperatura não encontrada!";
                            FeelsLikeResult.Text = "Sensação térmica não encontrada!";
                            HumidityResult.Text = "Humidade não encontrada!";
                            DescriptionResult.Text = "Detalhes não encontrados!";
                            WindResult.Text = "Informações sobre vento não encontradas!";
                            return;
                        }

                        // verificação defensiva para garantir que os dados esperados existem antes de acessá-los
                        if (weather?.Main == null || weather.Weather == null || weather.Weather.Length == 0)
                        {
                            CityResult.Text = "Dados incompletos!";
                            TempResult.Text = "";
                            FeelsLikeResult.Text = "";
                            HumidityResult.Text = "";
                            DescriptionResult.Text = "";
                            WindResult.Text = "";
                        }

                        // condicionais para alterar as imagens no WeatherImg conforme o retorno de description (Ex: description retornando 'nublado' exibe uma imagem de céu nublado)

                        // céu limpo
                        if (description.Contains("céu limpo")) 
                        {
                            // condição ? valorSeTrue : valorSeFalse
                            resourcePath = isDay ? "cleanSkyDay.jpg" : "cleanSkyNight.jpg";
                        }

                        // poucas nuvens / nuvens dispersas
                        else if (description.Contains("poucas nuvens") || description.Contains("nuvens dispersas") || description.Contains("algumas nuvens"))
                        {
                            resourcePath = isDay ? "fewCloudsDay.jpg" : "fewCloudsNight.jpg";
                        }

                        // nublado
                        else if (description.Contains("nublado"))
                        {
                            resourcePath = isDay ? "cloudySkyDay.jpg" : "cloudySkyNight.jpg"; 
                        }

                        // chuva leve / chuva
                        else if (description.Contains("chuva leve") || description.Contains("chuva"))
                        {
                            resourcePath = isDay ? "rainDay.jpg" : "rainNight.jpg";
                        }

                        // trovoada
                        else if (description.Contains("trovoada"))
                        {
                            resourcePath = isDay ? "thunderstormDay.jpg" : "thunderstormNight.jpg";
                        }                        

                        // neve / névoa
                        else if (description.Contains("neve") || description.Contains("névoa") || description.Contains("neve leve"))
                        {
                            resourcePath = isDay ? "snowDay.jpg" : "snowNight.jpg";
                        }

                        // imagem padrão da aplicação caso nenhuma das alternativas seja aceita
                        else
                        {
                            resourcePath = "default.jpg";
                        }

                        // chamando a imagem padrão caso não haja retorno válido
                        WeatherImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Resources/{resourcePath}", UriKind.Absolute));

                }

                catch (Exception ex)

                {
                    // em um cenário onde aconteça um erro, mostra na UI essa mensagem genérica
                    CityResult.Text = "Erro inesperado!";
                    TempResult.Text = "";
                    FeelsLikeResult.Text = "";
                    HumidityResult.Text = "";
                    DescriptionResult.Text = "";
                    WindResult.Text = "";

                    // para depuração local, logando ex.Message/stacktrace em log file ou Debug.WriteLine
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                }
            }
         
            // manipulador de funcionalidade para "placeholder" entre o SearchBox e o SearchTextBlock
            private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
            {
                SearchTextBlock.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Hidden;
            }

            // manipulador do botão para minimizar a aplicação
            private void MinimizeButton_Click(object sender, RoutedEventArgs e)
            {
                Application.Current.MainWindow.WindowState = WindowState.Minimized;                
            }

            // manipulador do botão para fechar a aplicação
            private void CloseButton_Click(object sender, RoutedEventArgs e)
            {
                Window.GetWindow(this)?.Close();
            }
    }
}