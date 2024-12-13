using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ApiClient: MonoBehaviour
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl = "http://localhost/tfg/Api_MySQL_TFG.php";
    private readonly string _apiKey = "";

    public ApiClient()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task SendQuery_SQL(string type, string input)
    {
        try
        {
            string url = _apiUrl;
            HttpResponseMessage response = null;

            Debug.Log($"SQL Petition: {input} Type: {type}");

            switch (type.ToUpper())
            {
                case "GET":
                    url += $"?{input}"; // Añadir parámetros a la URL
                    response = await _httpClient.GetAsync(url);
                    break;

                case "POST":
                    response = await _httpClient.PostAsync(url, new StringContent(input, Encoding.UTF8, "application/json"));
                    break;

                case "PUT":
                    HttpRequestMessage putRequest = new HttpRequestMessage(HttpMethod.Put, url)
                    {
                        Content = new StringContent(input, Encoding.UTF8, "application/json")
                    };
                    response = await _httpClient.SendAsync(putRequest);
                    break;

                case "DELETE":
                    HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, url)
                    {
                        Content = new StringContent(input, Encoding.UTF8, "application/json")
                    };
                    response = await _httpClient.SendAsync(deleteRequest);
                    break;

                default:
                    Debug.Log("Método no soportado.");
                    return;
            }

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("SQL Realizado: " + await response.Content.ReadAsStringAsync());
            }
            else
            {
                Debug.Log($"Error al realizar la consulta: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log("SQL Error: " + ex.Message);
        }
    }
}
