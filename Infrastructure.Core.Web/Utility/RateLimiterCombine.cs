using System.Threading.RateLimiting;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Web.Utility;

public static class RateLimiterCombine
{
    public static RateLimiter AndAll(params RateLimiter[] limiters)
        => AndAll((IEnumerable<RateLimiter>)limiters);

    public static RateLimiter AndAll(IEnumerable<RateLimiter> limiters)
    {
        var list = (limiters ?? throw new ArgumentNullException(nameof(limiters))).ToList();
        if (list.Count == 0)
            throw new ArgumentException("At least one limiter required.", nameof(limiters));
        if (list.Count == 1)
            return list[0];

        return new MultiRateLimiter(list);
    }

    private sealed class MultiRateLimiter : RateLimiter
    {
        private readonly IReadOnlyList<RateLimiter> _limiters;

        public MultiRateLimiter(IReadOnlyList<RateLimiter> limiters)
            => _limiters = limiters;

        protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken ct)
        {
            var acquired = new List<RateLimitLease>(_limiters.Count);

            foreach (var rl in _limiters)
            {
                var lease = await rl.AcquireAsync(permitCount, ct).ConfigureAwait(false);
                if (!lease.IsAcquired)
                {
                    foreach (var a in acquired) a.Dispose();
                    return lease;
                }
                acquired.Add(lease);
            }

            return new MultiLease(acquired);
        }

        protected override RateLimitLease AttemptAcquireCore(int permitCount)
        {
            var acquired = new List<RateLimitLease>(_limiters.Count);

            foreach (var rl in _limiters)
            {
                var lease = rl.AttemptAcquire(permitCount);
                if (!lease.IsAcquired)
                {
                    foreach (var a in acquired) a.Dispose();
                    return lease;
                }
                acquired.Add(lease);
            }

            return new MultiLease(acquired);
        }

        public override TimeSpan? IdleDuration => null;
        public override RateLimiterStatistics GetStatistics() => null;
    }

    private sealed class MultiLease : RateLimitLease
    {
        private readonly IReadOnlyList<RateLimitLease> _leases;

        public MultiLease(IReadOnlyList<RateLimitLease> leases)
            => _leases = leases;

        public override bool IsAcquired => true;

        public override IEnumerable<string> MetadataNames =>
            _leases.SelectMany(l => l.MetadataNames ?? Array.Empty<string>()).Distinct();

        public override bool TryGetMetadata(string metadataName, out object metadata)
        {
            foreach (var l in _leases)
            {
                if (l.TryGetMetadata(metadataName, out metadata))
                    return true;
            }

            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var l in _leases)
                    l.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}