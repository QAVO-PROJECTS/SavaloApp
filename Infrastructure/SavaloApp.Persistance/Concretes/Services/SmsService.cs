using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Application.Settings;


public class SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly SmsSettings _settings;

    public SmsService(HttpClient httpClient, IOptions<SmsSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
    {
        var url = $"{_settings.BaseUrl}/gateway/api/sms/v1/message/send?publicKey={_settings.PublicKey}";

        var payload = new
        {
            Text = $"Savolo təsdiq kodunuz: {otpCode}",
            Purpose = "INF",
            Options = new
            {
                Originator = _settings.Originator,
                SendTime = (string)null,
                ExpireTime = (string)null,
                Encoding = "LATIN",
                SmsType = "SMS",
                ReportLabel = _settings.ReportLabel
            },
            Receivers = new[]
            {
                new
                {
                    Receiver = phoneNumber
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.BearerToken);

        var response = await _httpClient.PostAsync(url, content);
        return response.IsSuccessStatusCode;
    }
}