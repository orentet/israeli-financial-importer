using System;

namespace IsraeliFinancialImporter.Types
{
    public class FinancialTransaction
    {
        public FinancialTransaction(string id, DateTime occuredAt, decimal amount, Currency currency,
            string description, bool isCompleted, Installment installment)
        {
            Id = id;
            OccuredAt = occuredAt;
            Amount = amount;
            Currency = currency;
            Description = description;
            IsCompleted = isCompleted;
            Installment = installment;
        }

        public string Id { get; }
        public DateTime OccuredAt { get; }
        public decimal Amount { get; }
        public Currency Currency { get; }
        public string Description { get; }
        public bool IsCompleted { get; }
        public Installment Installment { get; }
    }
}