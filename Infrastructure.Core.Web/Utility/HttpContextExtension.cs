using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Utility;

public static class HttpContextExtension
{
    public static string ResolveCorrelationId(this HttpContext ctx)
    {
        return ctx.TraceIdentifier ?? Activity.Current?.Id;
    }

    public static string? ResolveAccountId(this HttpContext ctx)
    {
        var u = ctx.User;

        if (u?.Identity?.IsAuthenticated == true)
        {
            var id = u.FindFirstValue("sub")
                  ?? u.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? u.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? u.FindFirstValue("oid")
                  ?? u.FindFirstValue("uid")
                  ?? u.Identity?.Name
                  ?? u.FindFirstValue(ClaimTypes.Email);

            return string.IsNullOrWhiteSpace(id) ? null : id.Trim().ToLowerInvariant();
        }

        var apiKey = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(apiKey))
            return apiKey!.Trim().ToLowerInvariant();

        return null;
    }

    public static string ResolveAccountIdOrAnon(this HttpContext ctx)
    {
        return ResolveAccountId(ctx) ?? "anon";
    }

    public static string ResolveActionPath(this HttpContext ctx)
        => ctx.Request.Path;

    public static string ResolveAccountIdOrAnonBucketIdByActionKey(this HttpContext ctx)
        => $"{ResolveActionPath(ctx)}:{ResolveAccountIdOrAnonBucketIdKey(ctx)}";

    public static string ResolveAccountIdOrAnonBucketIdKey(this HttpContext ctx)
        => ResolveAccountId(ctx) ?? $"anon:{ResolveBucketIp(ctx)}";

    public static string ResolveAccountIdAndBucketIpKey(this HttpContext ctx)
    {
        var acc = ResolveAccountIdOrAnon(ctx);
        var ipk = ResolveBucketIp(ctx);
        return $"accip:{acc}|{ipk}";
    }

    public static string ResolveIpOrAnon(this HttpContext ctx)
    {
        return ResolveIp(ctx) ?? "anon";
    }

    public static string? ResolveIp(this HttpContext ctx)
    {
        var ip = ctx.Connection.RemoteIpAddress;

        if (ip is null)
            return null;

        if (ip.IsIPv4MappedToIPv6)
            ip = ip.MapToIPv4();

        return ip.ToString();
    }

    public static string? ResolveBucketIp(this HttpContext ctx)
    {
        var ip = ctx.Connection.RemoteIpAddress;

        if (ip == null)
            return null;

        return BucketIp(ip);
    }

    public static string? BucketIp(this IPAddress ip, int ipv4Prefix = 24, int ipv6Prefix = 64)
    {
        if (ip == null) return null;

        if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
        var bytes = ip.GetAddressBytes();

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            return $"{bytes[0]}.{bytes[1]}.{bytes[2 & (ipv4Prefix >= 24 ? 2 : 0)]}.{(ipv4Prefix >= 24 ? 0 : bytes[3])}/{ipv4Prefix}";

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            var b = (byte[])bytes.Clone();

            var keepBits = Math.Clamp(ipv6Prefix, 0, 128);
            for (var bit = keepBits; bit < 128; bit++)
            {
                var i = bit / 8;
                var bitInByte = 7 - (bit % 8);
                b[i] &= (byte)~(1 << bitInByte);
            }
            return $"{new IPAddress(b)}/{ipv6Prefix}";
        }

        return ip.ToString();
    }
}