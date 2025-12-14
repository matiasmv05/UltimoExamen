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
    public interface IUserService
    {
        Task<ResponseData> GetAllUsers(UserQueryFilter filters);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task UpdateWalletAsync(int userId, decimal amount);

        Task<User> GetByIdAsync(int id);
        Task AddAsync(User user);
        Task Update(User user);
        Task Delete(int id);
    }
}
