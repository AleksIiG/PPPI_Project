using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
    public class RevorkedToken
    {
        public string Jti { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
    }
}