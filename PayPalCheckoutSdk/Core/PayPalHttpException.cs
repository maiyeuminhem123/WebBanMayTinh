
namespace PayPalCheckoutSdk.Core
{
    [Serializable]
    internal class PayPalHttpException : Exception
    {
        public PayPalHttpException()
        {
        }

        public PayPalHttpException(string? message) : base(message)
        {
        }

        public PayPalHttpException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}