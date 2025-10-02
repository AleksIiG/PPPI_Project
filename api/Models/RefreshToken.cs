using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class RefreshToken
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; } = null!;
        public bool IsRevoked { get; set; }
        public DateTime? Revoked { get; set; } = null;
        public string? RevokedByIp { get; set; } = null;
    }

}