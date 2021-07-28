using System.Collections.Generic;

namespace IsraeliFinancialImporter.Types
{
    public class FinancialAccount
    {
        public FinancialAccount(string id, IEnumerable<FinancialTransaction> transactions)
        {
            Id = id;
            Transactions = transactions;
        }

        public string Id { get; }

        public IEnumerable<FinancialTransaction> Transactions { get; }
    }
}