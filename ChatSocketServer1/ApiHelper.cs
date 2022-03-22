using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatSocketServer1
{
    public static class ApiHelper
    {
        public static HttpClient ApiClient { get; set; }

        public static void Initializer()
        {
            ApiClient = new HttpClient();
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
    public class weatherProcessor
    {
        public static async Task<string> weatherApi(String location)
        {
            string url = "";
            if(location != null && location.Contains(","))
            {
                
                string[] splitter = location.Split(',');
                string city = splitter[0];
                string country = splitter[1];
                

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://community-open-weather-map.p.rapidapi.com/weather?q={ city }%2C{ country }&lat=0&lon=0&id=2172797&lang=null&units=metric"),
                    Headers =
                {
                   { "x-rapidapi-host", "community-open-weather-map.p.rapidapi.com" },
                    { "x-rapidapi-key", "36f6964508msh98f7fe73e21a999p1992ddjsn5056b34a18a1" },
                },
                };

                using (var response = await ApiHelper.ApiClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string climate = await response.Content.ReadAsStringAsync();

                        dynamic dobj = JsonConvert.DeserializeObject<dynamic>(climate);
                        string temp = string.Concat("Temperatura: "+dobj["main"]["temp"].ToString() + "C" + " Sensacion Termica: " + dobj["main"]["feels_like"].ToString() + "C" + " Humedad: " + dobj["main"]["humidity"].ToString() +"%"+ " Clima: " + dobj["weather"][0]["description"].ToString());
                        return temp;
                    }
                    else
                    {
                        throw new Exception(response.ReasonPhrase);
                    }
                }
            }
            else
            {
                return ("Ubicacion Invalida"); 
            }
           
           

        }
    }
    public class exchangeProcessor { 
        public static async Task<string> exchangeApi(string values)
        {
            string url = "";
            if(values != null && values.Contains("a"))
            {
                string[] splitter = values.Split(' ');
                string total = splitter[0];
                string moneda1 = splitter[1];
                string moneda2 = splitter[3];
                moneda1 = moneda1.Trim(' ');
                moneda2 = moneda2.Trim(' ');

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://v6.exchangerate-api.com/v6/3b17005dab7bb92eeaac54c5/pair/{ moneda1 }/{ moneda2 }/{ total }"),
                };
                using (var response = await ApiHelper.ApiClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string exchange = await response.Content.ReadAsStringAsync();

                        dynamic dobj = JsonConvert.DeserializeObject<dynamic>(exchange);
                        string val = string.Concat(dobj["conversion_result"].ToString() +" "+ dobj["target_code"].ToString());
                        return val;
                    }
                    else
                    {
                        return ("Input invalido");
                    }
                }
            }
            else
            {
                return ("Input invalido");
            }
        }
    }

}
