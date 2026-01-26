namespace WeatherApp.Models;

/*
 
Estrutura de informações de clima (outras informações) na API:

    weather (more info Weather condition codes)
        weather.id Weather condition id
        weather.main Group of weather parameters (Rain, Snow, Clouds etc.)
        weather.description Weather condition within the group. Please find more here. You can get the output in your language. Learn more
        weather.icon Weather icon id

*/

public class WeatherInfo
{
    // descrição textual do tempo no JSON (ex: "céu limpo", "chuva leve", etc.)
    public string Description { get; set; }
}