using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using NUnit.Framework;

using SoftwaredeveloperDotAt.Infrastructure.Core;
using SoftwaredeveloperDotAt.Infrastructure.Core.AsyncTasks;
using SoftwaredeveloperDotAt.Infrastructure.Core.EntityFramework;

namespace Infrastructure.Core.Tests
{
    public abstract class BaseTest<TDomainStartup>
        where TDomainStartup : IDomainStartupCore, new()
    {
        protected IServiceScope _serviceScope;

        protected IConfigurationRoot _configuration;
        protected IHostBuilder _hostBuilder;
        private TDomainStartup _domainStartup;
        protected IDbContext _context;

        //protected IDBContext _nadaContext;
        //protected IDbContextTransaction _dbTransaction;

        public enum DbTransactionModeType
        {
            None,
            OnePerTest
        }

        protected virtual DbTransactionModeType UseDbTransactionMode { get; } = DbTransactionModeType.OnePerTest;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
        }

        [SetUp]
        public virtual void SetUp()
        {
            BuildHost();
            BuildRootServiceScope();

        }

        [TearDown]
        public virtual void TearDown()
        {
            //if (UseDbTransactionMode == DbTransactionModeType.OnePerTest)
            //{
            //    _dbTransaction?.Rollback();
            //    _dbTransaction?.Dispose();
            //    _dbTransaction = null;
            //}

            //_nadaContext?.Database?.CloseConnection();
            //_nadaContext?.Dispose();
            //_nadaContext = null;

            _serviceScope?.Dispose();
            _serviceScope = null;
        }

        protected void BuildRootServiceScope()
        {
            var host = _hostBuilder.Build();
            _domainStartup.ConfigureApp(host);

            _serviceScope = host.Services.CreateScope();

            _context = _serviceScope.ServiceProvider
                    .GetRequiredService<IDbContext>();

            //_nadaContext = _serviceScope.ServiceProvider.GetService<NadaContext>();

            //if (UseDbTransactionMode == DbTransactionModeType.OnePerTest)
            //    _dbTransaction = _nadaContext.Database.BeginTransaction();
            //else
            //    _dbTransaction = null;
        }

        protected virtual IHostBuilder BuildHost()
        {
            _configuration = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var hostEnvironmentMock = new Mock<IHostEnvironment>();
            hostEnvironmentMock
                .SetupGet(p => p.EnvironmentName)
                .Returns(() => Environments.Development);
            var hostEnvironment = hostEnvironmentMock.Object;

            _domainStartup = new TDomainStartup();

            _hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.AddConfiguration(_configuration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    _domainStartup.ConfigureServices(services, hostContext.Configuration, hostEnvironment);

                    CreateServiceCollection(services, hostEnvironment, _configuration);
                });

            return _hostBuilder;
        }

        protected virtual void CreateServiceCollection(IServiceCollection services, IHostEnvironment hostEnvironment, IConfigurationRoot configuration)
        {
            services.AddScoped<ICurrentUserService, AlwaysServiceUserCurrentUserService>();
        }

        protected async Task WaitUntilReferencedAsyncTasksFinished(Guid[] referenceIds, int? timeoutInSeconds = null, CancellationToken cancellationToken = default)
        {
            using var serivceScope = _serviceScope.ServiceProvider.CreateScope();

            var asyncTaskExecutor = serivceScope.ServiceProvider
                    .GetRequiredService<AsyncTaskExecutor>();

            await asyncTaskExecutor.ExecuteBatchAsync(100, cancellationToken);

            await asyncTaskExecutor
                    .WaitUntilReferencedAsyncTasksFinished(referenceIds, timeoutInSeconds, cancellationToken);
        }
    }
}
