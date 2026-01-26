namespace WeatherApp.Models;

/*
 
Estrutura de informações de vento na API:

    wind
        wind.speed Wind speed. Unit Default: meter/sec, Metric: meter/sec, Imperial: miles/hour
        wind.deg Wind direction, degrees (meteorological)
        wind.gust Wind gust. Unit Default: meter/sec, Metric: meter/sec, Imperial: miles/hour

*/

public class WindInfo
{
    public double Speed { get; set; }
}