using System;
using System.Net;


namespace SWIMS.Helpers.Net
{
    public static class CidrMatcher
    {
        // IPv4 only simple matcher (enough for intranet ranges)
        public static bool IsInCidrs(string? ipString, string[] cidrs)
        {
            if (string.IsNullOrWhiteSpace(ipString) || cidrs == null || cidrs.Length == 0) return false;
            if (!IPAddress.TryParse(ipString, out var ip)) return false;
            if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return false;


            var ipBytes = ip.GetAddressBytes();
            uint ipInt = ((uint)ipBytes[0] << 24) | ((uint)ipBytes[1] << 16) | ((uint)ipBytes[2] << 8) | ipBytes[3];


            foreach (var cidr in cidrs)
            {
                var parts = cidr.Split('/');
                if (parts.Length != 2) continue;
                if (!IPAddress.TryParse(parts[0], out var net)) continue;
                if (!int.TryParse(parts[1], out var prefix)) continue;


                var netBytes = net.GetAddressBytes();
                uint netInt = ((uint)netBytes[0] << 24) | ((uint)netBytes[1] << 16) | ((uint)netBytes[2] << 8) | netBytes[3];
                uint mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
                if ((ipInt & mask) == (netInt & mask)) return true;
            }
            return false;
        }
    }
}