﻿using System.Net.Http.Json;
using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddEnvironmentVariables()
  .Build();


var servicesConfig = config.GetSection("Sevices");


var tempServiceConfig = servicesConfig.GetSection("Temperature");
var tempServiceHost = tempServiceConfig["Host"];
var tempServicePort = tempServiceConfig["Port"];

var precipServiceConfig = servicesConfig.GetSection("Precipitation");
var precipServiceHost = precipServiceConfig["Host"];
var precipServicePort = precipServiceConfig["Port"];


var zipCodes = new List<string> {
  "73026",
  "13406",
  "61426",
  "03026",
  "54026",
};

Console.WriteLine("Starting Data Load");

var temperatureHttpClient = new HttpClient();
temperatureHttpClient.BaseAddress = new Uri($"http://{tempServiceHost}:{tempServicePort}");

var precipitationHttpClient = new HttpClient();
precipitationHttpClient.BaseAddress = new Uri($"http://{precipServiceHost}:{precipServicePort}");

foreach (var zip in zipCodes) 
{
  Console.WriteLine($"Processing Zip Code: {zip}");

  var from = DateTime.Now.AddYears(-2);
  var thru = DateTime.Now;

  for(var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
  {
    var temps = PostTemp(zip, day, temperatureHttpClient);
    PostPrecip(temps[0], zip, day, precipitationHttpClient);
  }
}

List<int> PostTemp(string zip, DateTime day, HttpClient httpClient)
{
  var rand = new Random();
  var t1 = rand.Next(0, 100);
  var t2 = rand.Next(0, 100);
  var hiloTemps = new List<int> { t1, t2 };
  hiloTemps.Sort();

  var temperatureObservation = new TemperatureModel 
  {
    TempLowF = hiloTemps[0],
    TempHighF = hiloTemps[1],
    ZipCode = zip,
    CreatedOn = day
  };

  var tempResponse = httpClient
  .PostAsJsonAsync("observation", temperatureObservation)
  .Result;

  if (tempResponse.IsSuccessStatusCode)
  {
    Console.Write($"Posted Temperature: Date: {day:d} " +
                  $"Zip: {zip} " +
                  $"Lo (F): {hiloTemps[0]} " +
                  $"Hi (F): {hiloTemps[1]}"
                  );
  }
  else
  {
    Console.WriteLine(tempResponse.ToString());
  }
  return hiloTemps;
}

void PostPrecip(int lowTemp, string zip, DateTime day, HttpClient httpClient)
{
  var rand = new Random();
  var isPrecip = rand.Next(2) < 1;
  PrecipitationModel precipitation;

  if (isPrecip)
  {
    var precipInches = rand.Next(1, 16);
    if (lowTemp < 32)
    {
      precipitation = new PrecipitationModel 
      {
        AmountInches = precipInches,
        WeatherType = "snow",
        ZipCode = zip,
        CreatedOn = day
      };
    }
    else 
    {
      precipitation = new PrecipitationModel 
      {
        AmountInches = precipInches,
        WeatherType = "rain",
        ZipCode = zip,
        CreatedOn = day
      };
    }
  }
  else
  {
    precipitation = new PrecipitationModel
    {
      AmountInches = 0,
      WeatherType = "none",
      ZipCode = zip,
      CreatedOn = day
    };
  }


  var precipResponse = httpClient
    .PostAsJsonAsync("observation", precipitation)
    .Result;

  if (precipResponse.IsSuccessStatusCode)
  {
    Console.Write($"Posted Precipitation: Date {day:d} " +
                  $"Zip: {zip} " +
                  $"Type: {precipitation.WeatherType} " +
                  $"Amount (in.): {precipitation.AmountInches}"
                );
  }
}
