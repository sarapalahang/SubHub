using System;
using System.Collections.Generic;
using System.Text;

namespace SubHub.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // مقداردهی اولیه
        public string PhoneNumber { get; set; } = string.Empty; // مقداردهی اولیه
        public string? Email { get; set; }
        public DateTime? CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
