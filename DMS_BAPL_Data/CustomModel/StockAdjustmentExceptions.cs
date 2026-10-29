namespace DMS_BAPL_Data.CustomModel
{
    public sealed class StockAdjustmentValidationException : Exception
    {
        public StockAdjustmentValidationException(string message) : base(message)
        {
        }
    }

    public sealed class StockAdjustmentConflictException : Exception
    {
        public StockAdjustmentConflictException(string message) : base(message)
        {
        }
    }
}
