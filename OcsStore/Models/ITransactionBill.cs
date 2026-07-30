namespace OcsStore.Models
{
    public interface ITransactionBill
    {
        short Customer { get; set; }
        DateTime Date { get; set; }
        int Id { get; set; }
        string Time { get; set; }
        decimal TotalValue { get; set; }
        short UserCreated { get; set; }
    }
}