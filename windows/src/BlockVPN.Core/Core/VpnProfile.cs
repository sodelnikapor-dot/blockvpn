using System;

namespace BlockVPN.Core
{
    public class VpnProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; }
        public string Region { get; set; }
        public string ConfigText { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
