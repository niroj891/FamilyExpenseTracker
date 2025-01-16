namespace PersonalExpenseTracker2.model
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public int Amount { get; set; }
        public string Title { get; set; }
        public string TransactionTag { get; set; }

        public string TransactionType { get; set; }
    }
}
