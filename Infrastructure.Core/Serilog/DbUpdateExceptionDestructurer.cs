using Microsoft.EntityFrameworkCore;

using Serilog.Exceptions.Core;
using Serilog.Exceptions.Destructurers;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Serilog;

public class DbUpdateExceptionDestructurer : ExceptionDestructurer
{
    public override Type[] TargetTypes => [typeof(DbUpdateException), typeof(DbUpdateConcurrencyException)];

    public override void Destructure(
        Exception exception,
        IExceptionPropertiesBag propertiesBag,
        Func<Exception, IReadOnlyDictionary<string, object>> destructureException)
    {
        base.Destructure(exception, propertiesBag, destructureException);

        var dbUpdateException = (DbUpdateException)exception;
        var entriesValue = dbUpdateException.Entries?
            .Select(
                e => new
                {
                    DebugShortView = e.DebugView.ShortView,
                    DebugLongView = e.DebugView.LongView,
                })
            .ToList();
        propertiesBag.AddProperty(nameof(DbUpdateException.Entries), entriesValue);
    }
}
