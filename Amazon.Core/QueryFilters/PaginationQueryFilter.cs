using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.QueryFilters
{
    public abstract class PaginationQueryFilter
    {

        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
