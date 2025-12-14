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
    public interface IProductService
    {
        Task<ResponseData> GetAllProducts(ProductQueryFilter filters);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task Update(Product product);
        Task Delete(int id);
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        Task<IEnumerable<Product>> GetBySellerAsync(int sellerId);
    }
}
