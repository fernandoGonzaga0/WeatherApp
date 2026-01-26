using OpenWeatherMap.Models;

namespace WeatherApp.Models
{
    // classes para desserializar JSON
    public class WeatherResponse
    {
        // representa o campo "name" do JSON (nome da cidade)
        public string? Name { get; set; }

        // representa o nó "main" do JSON (temperatura, etc.)
        public MainInfo? Main { get; set; }

        // representa o array "weather" do JSON (descrição do tempo, etc.)
        public required WeatherInfo[] Weather { get; set; }

        // representa o campo wind no JSON
        public WindInfo Wind { get; set; }
    }

    public class WindInfo
    {
        public double Speed { get; set; } // velocidade do vento em m/s
    }
}