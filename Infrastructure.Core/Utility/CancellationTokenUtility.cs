namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class CancellationTokenUtility
{
    //[HttpGet]
    //public async Task<IActionResult> IsImportFinished(
    //    Guid importId,
    //    CancellationToken cancellationToken,
    //    [FromServices] IServiceScopeFactory scopeFactory)
    //{
    //    var cts = new CancellationTokenSource();
    //    cts.CancelAfter(TimeSpan.FromSeconds(60));

    //    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

    //    try
    //    {
    //        while (!linkedCts.Token.IsCancellationRequested)
    //        {
    //            using (var scope = scopeFactory.CreateScope())
    //            {
    //                var service = scope.ServiceProvider.GetRequiredService<ApplicationUsersImportCsvService>();

    //                if (await service.IsImportFinishedAsync(importId))
    //                {
    //                    return Ok(true);
    //                }
    //            }

    //            await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);
    //        }
    //    }
    //    catch (OperationCanceledException)
    //    {
    //        // Operation was cancelled due to timeout or client disconnecting
    //    }

    //    return NoContent();
    //}

}