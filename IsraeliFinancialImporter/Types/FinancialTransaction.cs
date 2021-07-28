using System;

namespace IsraeliFinancialImporter.Types
{
    public class FinancialTransaction
    {
        public FinancialTransaction(string id, DateTime occuredAt, decimal amount, Currency currency,
            string description, string memo, bool isCleared, Installment installment)
        {
            Id = id;
            OccuredAt = occuredAt;
            Amount = amount;
            Currency = currency;
            Description = description;
            Memo = memo;
            IsCleared = isCleared;
            Installment = installment;
        }

        public string Id { get; }
        public DateTime OccuredAt { get; }
        public decimal Amount { get; }
        public Currency Currency { get; }
        public string Description { get; }
        public string Memo { get; }
        public bool IsCleared { get; }
        public Installment Installment { get; }
    }
}