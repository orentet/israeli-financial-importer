using System;

namespace IsraeliFinancialImporter.Types
{
    public class FinancialTransaction
    {
        public FinancialTransaction(string id, string accountId, DateTime occuredAt, decimal amount, Currency currency,
            string payee, string memo, bool isCleared, Installment installment)
        {
            Id = id;
            AccountId = accountId;
            OccuredAt = occuredAt;
            Amount = amount;
            Currency = currency;
            Payee = payee;
            Memo = memo;
            IsCleared = isCleared;
            Installment = installment;
        }

        /// <summary>
        ///     the ID is internal and unique for each importer
        /// </summary>
        public string Id { get; }

        public string AccountId { get; }
        public DateTime OccuredAt { get; }
        public decimal Amount { get; }
        public Currency Currency { get; }
        public string Payee { get; }
        public string Memo { get; }
        public bool IsCleared { get; }
        public Installment Installment { get; }
    }
}