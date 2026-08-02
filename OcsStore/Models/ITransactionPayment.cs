namespace OcsStore.Models
{
    public interface ITransactionPayment
    {
        decimal Amount { get; set; }
        short Customer { get; set; }
        DateOnly Date { get; set; }
        int Id { get; set; }
        bool? IsCompleted { get; set; }
        string Time { get; set; }
        short? UserCreated { get; set; }
    }
}