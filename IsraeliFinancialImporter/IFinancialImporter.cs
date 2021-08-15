using System;
using System.Collections.Generic;
using IsraeliFinancialImporter.Types;

namespace IsraeliFinancialImporter
{
    public interface IFinancialImporter : IDisposable
    {
        IEnumerable<FinancialTransaction> Import(DateTime fromInclusive, DateTime toInclusive);
    }
}