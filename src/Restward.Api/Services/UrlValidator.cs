using System.Net;
using System.Net.Sockets;

namespace Restward.Api.Services;

public static class UrlValidator
{
    public static async Task ValidateUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid URL format");

        if (uri.Scheme != "http" && uri.Scheme != "https")
            throw new ArgumentException("URL must use HTTP or HTTPS scheme");

        // Resolve DNS and check all IPs
        IPAddress[] addresses;
        try
        {
            addresses = await Dns.GetHostAddressesAsync(uri.Host);
        }
        catch (SocketException)
        {
            throw new ArgumentException($"Could not resolve host: {uri.Host}");
        }

        if (addresses.Length == 0)
            throw new ArgumentException($"Could not resolve host: {uri.Host}");

        foreach (var ip in addresses)
        {
            if (IsPrivateOrReservedIp(ip))
                throw new ArgumentException($"Requests to private or reserved IP addresses are not allowed");
        }
    }

    public static bool IsPrivateOrReservedIp(IPAddress ip)
    {
        if (IPAddress.IsLoopback(ip))
            return true;

        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // ::1 is covered by IsLoopback
            // fc00::/7 (unique local)
            var bytes = ip.GetAddressBytes();
            if ((bytes[0] & 0xFE) == 0xFC)
                return true;

            // fe80::/10 (link-local)
            if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80)
                return true;

            // Map IPv6-mapped IPv4 to check IPv4 ranges
            if (ip.IsIPv4MappedToIPv6)
                return IsPrivateOrReservedIp(ip.MapToIPv4());

            return false;
        }

        var ipBytes = ip.GetAddressBytes();

        // 0.0.0.0/8
        if (ipBytes[0] == 0)
            return true;

        // 10.0.0.0/8
        if (ipBytes[0] == 10)
            return true;

        // 172.16.0.0/12
        if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31)
            return true;

        // 192.168.0.0/16
        if (ipBytes[0] == 192 && ipBytes[1] == 168)
            return true;

        // 169.254.0.0/16 (link-local)
        if (ipBytes[0] == 169 && ipBytes[1] == 254)
            return true;

        // 100.64.0.0/10 (RFC 6598 - shared address space)
        if (ipBytes[0] == 100 && ipBytes[1] >= 64 && ipBytes[1] <= 127)
            return true;

        // 192.0.2.0/24, 198.51.100.0/24, 203.0.113.0/24 (RFC 5737 - documentation)
        if (ipBytes[0] == 192 && ipBytes[1] == 0 && ipBytes[2] == 2)
            return true;
        if (ipBytes[0] == 198 && ipBytes[1] == 51 && ipBytes[2] == 100)
            return true;
        if (ipBytes[0] == 203 && ipBytes[1] == 0 && ipBytes[2] == 113)
            return true;

        return false;
    }
}
