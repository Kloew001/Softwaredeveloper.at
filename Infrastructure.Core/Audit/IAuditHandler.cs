namespace SoftwaredeveloperDotAt.Infrastructure.Core.Audit
{
    public interface IAuditHandler : ITypedSingletonService<IAuditHandler>
    {
        void RegisterProvider();
    }
}
