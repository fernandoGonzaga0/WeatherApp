namespace WeatherApp.Models
{
    // classes para desserializar JSON (aqui estamos pensando em objetos para chamada em MainWindow.xaml.cs, visando obter por final o dado da API e inserir no TextBlock correspondente
    public class WeatherResponse
    {
        public string? Name { get; set; } // representa o campo "name" do JSON (nome da cidade)
                                          // como na API esse campo Name é uma string direta (sem ser uma classe, como Main, Weather, Wind,), podemos criar diretamente a string

        public int Timezone { get; set; } // representa Timezone (horário local) no JSON em segundos, esse valor será convertido no programa
        
        public MainInfo? Main { get; set; } // representa a classe "main" do JSON (temperatura, etc.)
                                           // vamos usar duas propriedades aqui também, assim como em WeatherInfo abaixo, mas como a classe Main se trata apenas de double, podemos chamar
                                           // ela diretamente ao invés de alocar por array

        public required WeatherInfo[] Weather { get; set; } // representa a classe "weather" do JSON (descrição do tempo, etc.)

        public WindInfo? Wind { get; set; } // representa a classe wind no JSON

    }
}