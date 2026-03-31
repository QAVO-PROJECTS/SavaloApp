namespace SavaloApp.Application.Dtos.Goal;

public class UpdateGoalDto
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyAccountId { get; set; }


    public  string? StartDate { get; set; }
    public  string? EndDate { get; set; }
    public string? ColorCode { get; set; }
    public string? IconId { get; set; }
}