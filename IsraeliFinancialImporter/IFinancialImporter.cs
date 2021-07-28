using System;
using System.Collections.Generic;
using IsraeliFinancialImporter.Types;

namespace IsraeliFinancialImporter
{
    public interface IFinancialImporter
    {
        IEnumerable<FinancialAccount> Import(DateTime fromInclusive, DateTime toInclusive);
    }
}