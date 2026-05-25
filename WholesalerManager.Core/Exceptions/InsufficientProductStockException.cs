namespace WholesalerManager.Core.Exceptions
{
    public class InsufficientProductStockException : ArgumentException
    {
        public InsufficientProductStockException() : base()
        {
        }

        public InsufficientProductStockException(string? message) : base(message)
        {
        }

        public InsufficientProductStockException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
