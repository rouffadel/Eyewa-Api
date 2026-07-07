using System;

namespace Eyewa.Domain.Entities
{
    public class DbLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty; // Info, Error, Warning
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Exception { get; set; }
    }
}
