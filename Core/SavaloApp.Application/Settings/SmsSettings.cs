namespace SavaloApp.Application.Settings;

public class SmsSettings
{
    public string BaseUrl { get; set; }
    public string PublicKey { get; set; }
    public string BearerToken { get; set; }
    public string Originator { get; set; }
    public string ReportLabel { get; set; }
}