namespace OcsStore.Models
{
    public class MainReport
    {
        public decimal BillTotal { get; set; }
        public decimal CustomerDebtTotal { get; set; }
        public decimal Stock1Value { get; set; }
        public decimal Stock2Value { get; set; }
        public decimal RecevingTotal { get; set; }
        public decimal TotalExpense { get; set; }

        public decimal TotalCost
        {
            get { return RecevingTotal + TotalExpense; }
        }
        public decimal TotalProfit
        {
            get { return BillTotal - TotalCost; }
        }
    }
}
