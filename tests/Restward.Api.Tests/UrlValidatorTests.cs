using System.Net;
using Restward.Api.Services;

namespace Restward.Api.Tests;

public class UrlValidatorTests
{
    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("127.0.0.2")]
    [InlineData("10.0.0.1")]
    [InlineData("10.255.255.255")]
    [InlineData("172.16.0.1")]
    [InlineData("172.31.255.255")]
    [InlineData("192.168.0.1")]
    [InlineData("192.168.255.255")]
    [InlineData("169.254.1.1")]
    [InlineData("0.0.0.0")]
    [InlineData("100.64.0.1")]
    [InlineData("192.0.2.1")]
    [InlineData("198.51.100.1")]
    [InlineData("203.0.113.1")]
    public void IsPrivateOrReservedIp_ReturnsTrue_ForPrivateIps(string ipString)
    {
        var ip = IPAddress.Parse(ipString);
        Assert.True(UrlValidator.IsPrivateOrReservedIp(ip));
    }

    [Theory]
    [InlineData("8.8.8.8")]
    [InlineData("1.1.1.1")]
    [InlineData("93.184.216.34")]
    [InlineData("172.32.0.1")]
    [InlineData("192.169.0.1")]
    public void IsPrivateOrReservedIp_ReturnsFalse_ForPublicIps(string ipString)
    {
        var ip = IPAddress.Parse(ipString);
        Assert.False(UrlValidator.IsPrivateOrReservedIp(ip));
    }

    [Theory]
    [InlineData("::1")]
    public void IsPrivateOrReservedIp_ReturnsTrue_ForIpv6Loopback(string ipString)
    {
        var ip = IPAddress.Parse(ipString);
        Assert.True(UrlValidator.IsPrivateOrReservedIp(ip));
    }

    [Theory]
    [InlineData("fc00::1")]
    [InlineData("fd00::1")]
    public void IsPrivateOrReservedIp_ReturnsTrue_ForIpv6UniqueLocal(string ipString)
    {
        var ip = IPAddress.Parse(ipString);
        Assert.True(UrlValidator.IsPrivateOrReservedIp(ip));
    }

    [Theory]
    [InlineData("fe80::1")]
    public void IsPrivateOrReservedIp_ReturnsTrue_ForIpv6LinkLocal(string ipString)
    {
        var ip = IPAddress.Parse(ipString);
        Assert.True(UrlValidator.IsPrivateOrReservedIp(ip));
    }

    [Fact]
    public async Task ValidateUrlAsync_ThrowsForInvalidScheme()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            UrlValidator.ValidateUrlAsync("ftp://example.com"));
    }

    [Fact]
    public async Task ValidateUrlAsync_ThrowsForInvalidUrl()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            UrlValidator.ValidateUrlAsync("not-a-url"));
    }

    [Fact]
    public async Task ValidateUrlAsync_ThrowsForLocalhostUrl()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            UrlValidator.ValidateUrlAsync("http://127.0.0.1/admin"));
    }
}
