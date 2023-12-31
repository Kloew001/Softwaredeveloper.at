namespace SoftwaredeveloperDotAt.Infrastructure.Core
{
    public interface ITransientService
    {

    }

    public interface IScopedService
    {

    }

    public interface ITypedScopedService<TTypeRegisterFor> : IScopedService
    {
    }

    public interface ISingletonService
    {

    }
    public interface ITypedSingletonService<TTypeRegisterFor> : ISingletonService
    {
    }

}
