<<<<<<< HEAD
﻿using Amazon.Core.CustomEntities;
=======
using Amazon.Core.CustomEntities;
>>>>>>> 76960dffaad66a3488e31e29060769f6f1fac94f
using Amazon.Core.Entities;
using Amazon.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Interface
{
    public interface IPaymentService
    {
        Task<ResponseData> GetAllPayments(PaymentQueryFilter filters);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment> GetByIdAsync(int id);
        Task AddAsync(Payment payment);
        Task Update(Payment payment);
        Task Delete(int id);
        Task<Payment> GetByOrderIdAsync(int orderId);
    }
}
