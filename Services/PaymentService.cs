using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyCourse.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;

        public PaymentService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PaymentModel>> GetPaymentHistoryByUserId(int userId, int page = 1, int pageSize = 10)
        {
            // Đảm bảo tham số hợp lệ
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            // Truy vấn dữ liệu với giới hạn số lượng và sắp xếp (mới nhất trước)
            var payments = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt) // Sắp xếp theo ngày thanh toán mới nhất
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.PaymentDetails)
                .AsNoTracking() // Tối ưu hiệu suất vì chỉ đọc dữ liệu
                .ToListAsync();

            // Chuyển đổi sang model và trả về
            return _mapper.Map<List<PaymentModel>>(payments);
        }

        public async Task<PaymentModel?> GetPaymentById(int paymentId)
        {
            var payment = await _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .Include(p => p.PaymentDetails)
                .FirstOrDefaultAsync();

            return _mapper.Map<PaymentModel?>(payment);
        }

        public async Task<bool> DeletePayment(int paymentId)
        {
            var paymentToDelete = await _context.Payments.FindAsync(paymentId);
            if (paymentToDelete != null)
            {
                _context.Payments.Remove(paymentToDelete);
                var affectedRows = await _context.SaveChangesAsync();
                return affectedRows > 0;
            }
            return false;
        }
    }
}