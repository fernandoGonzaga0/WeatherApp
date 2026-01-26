using System.Text.Json.Serialization;

namespace WeatherApp.Models;

/*

Estrutura de informações main na API:

    main
        main.temp Temperature. Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit
        main.feels_like Temperature. This temperature parameter accounts for the human perception of weather. Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit
        main.pressure Atmospheric pressure on the sea level, hPa
        main.humidity Humidity, %
        main.temp_min Minimum temperature at the moment. This is minimal currently observed temperature (within large megalopolises and urban areas). Please find more info here. Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit
        main.temp_max Maximum temperature at the moment. This is maximal currently observed temperature (within large megalopolises and urban areas). Please find more info here. Unit Default: Kelvin, Metric: Celsius, Imperial: Fahrenheit
        main.sea_level Atmospheric pressure on the sea level, hPa
        main.grnd_level Atmospheric pressure on the ground level, hPa

*/

public class MainInfo
{
    // temperatura atual no JSON 
    public double Temp { get; set; }

    // sensação térmica no local
    [JsonPropertyName("feels_like")] // necessário fazer essa tradução, pois na API o dado vem como feels_like, mas não quero mudar o padrão de nome aqui no projeto
    public double FeelsLike { get; set; }

    // humidade atual 
    public double Humidity { get; set; }
}