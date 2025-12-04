using Amazon.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Entities
{
    public partial class Security : BaseEntity
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public string Name { get; set; }

        public RoleType? Role { get; set; }
    }
}
