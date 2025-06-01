using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IPaymentService
    {
        Task<List<PaymentModel>> GetPaymentHistoryByUserId(int userId,int page,int pageSize);
        Task<PaymentModel?> GetPaymentById(int paymentId);
        Task<bool> DeletePayment(int paymentId);
    }
}