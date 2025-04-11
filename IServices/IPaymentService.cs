using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IPaymentService
    {

        Task<string> CreateVnPayPaymentUrl(VnPayRequest request);
      
    }
}
