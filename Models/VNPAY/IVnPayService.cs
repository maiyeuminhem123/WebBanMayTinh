namespace WebBanMayTinh.Models.VNPAY
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model );
        VnPaymentResponseModel PaymentExcute(IQueryCollection colections);
    }
}
