namespace SavaloApp.Persistance.Concretes.Services;

using System.Text;
using System.Text.Json;
using SavaloApp.Application.Abstracts.Services;

public class OtpSenderService : IOtpSenderService
{
    private readonly HttpClient _httpClient;

    public OtpSenderService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string otp)
    {
        var url = "https://savolosmswa-production.up.railway.app/send-otp";

        var body = new
        {
            phone = phoneNumber,
            otp = otp
        };

        var json = JsonSerializer.Serialize(body);

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-api-key", "savolo-secret-key");
        request.Headers.Add("accept", "application/json");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Response: {responseContent}");

        return response.IsSuccessStatusCode;
    }
}