namespace IsraeliFinancialImporter.Types
{
    public class Installment
    {
        public Installment(int current, int total)
        {
            Current = current;
            Total = total;
        }

        public int Current { get; }
        public int Total { get; }
    }
}