namespace OcsStore.Models
{
    public class MonthReport
    {
        public int PrevMonth { get; set; }
        public decimal SalesTotal { get; set; }
        public decimal PrevSalesTotal { get; set; }
        public decimal SalesQuantity { get; set; }
        public decimal PrevSalesQuantity { get; set; }
        public decimal CustomerDebt { get; set; }
        public decimal PrevCustomerDebt { get; set; }
        public decimal CostPercentage { get; set; }
        public decimal PrevCostPercentage { get; set; }

        public decimal TotalCost { get; set; }
        public decimal PrevTotalCost { get; set; }
        public decimal TotalProfit
        {
            get { return SalesTotal - TotalCost; }
        }
        public decimal PrevTotalProfit
        {
            get { return PrevSalesTotal - PrevTotalCost; }
        }
    }
}
