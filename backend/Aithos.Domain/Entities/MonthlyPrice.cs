namespace Aithos.Domain.Entities;

public class MonthlyPrice
{
    public int Id { get; set; }

    public DateTime Month { get; set; }

    public decimal ElectricityPrice { get; set; }
}
