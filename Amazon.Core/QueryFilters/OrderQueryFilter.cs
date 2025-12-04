using Amazon.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.QueryFilters
{
    public class OrderQueryFilter : PaginationQueryFilter
    {
        public int UserId { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime? UpdatedAt { get; set; }
       
    }
}
