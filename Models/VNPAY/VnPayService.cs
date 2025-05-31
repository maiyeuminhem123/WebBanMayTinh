using StackExchange.Redis;
using VNPAY.NET;

namespace WebBanMayTinh.Models.VNPAY
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;
        public VnPayService(IConfiguration config)
        {
            _config = config;
        }
        public string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model)
        {
            var tick = DateTime.Now.Ticks.ToString();
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);

            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + model.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VnPay:ReturnUrl"]);

            vnpay.AddRequestData("vnp_TxnRef", tick); // Mã tham chiếu của giao dịch tại hệ 

            var paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"], _config["VnPay:HashSecret"]);
            return paymentUrl;
        }

        public VnPaymentResponseModel PaymentExcute(IQueryCollection colections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in colections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }
            var vnp_oderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_transactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_secureHash = colections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value.ToString();
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnpayOrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            bool checkSignature = vnpay.ValidateSignature(vnp_secureHash, _config["VnPay:HashSecret"]);
            if (!checkSignature)
            {
                return new VnPaymentResponseModel
                {
                    Success = false
                };
            }
            return new VnPaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VNPAY",
                OrderDescription = vnpayOrderInfo,
                OrderId = vnp_oderId.ToString(),
                TransactionId = vnp_transactionId.ToString(),
                Token = vnp_secureHash,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }
}
