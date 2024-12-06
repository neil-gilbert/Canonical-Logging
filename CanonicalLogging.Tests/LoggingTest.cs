using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CanonicalLogging.Tests
{
    public class CanonicalLoggerTests
    {
        private readonly ILogger _testLogger;
        private readonly CanonicalLogger _canonicalLogger;

        public CanonicalLoggerTests()
        {
            var factory = LoggerFactory.Create(builder => builder.AddConsole());
            _testLogger = factory.CreateLogger("Test");
            _canonicalLogger = new CanonicalLogger("Test", _testLogger);
        }

        [Fact]
        public void Logger_CollectsEntries()
        {
            _canonicalLogger.LogInformation("Test message 1");
            _canonicalLogger.LogWarning("Test message 2");

            var entries = _canonicalLogger.FlushLogs().ToList();

            Assert.Equal(2, entries.Count);
            Assert.Equal(LogLevel.Information, entries[0].LogLevel);
            Assert.Equal(LogLevel.Warning, entries[1].LogLevel);
            Assert.Contains("Test message 1", entries[0].Message);
            Assert.Contains("Test message 2", entries[1].Message);
        }

        [Fact]
        public void Logger_HandlesScope()
        {
            using (_canonicalLogger.BeginScope("Scope1"))
            {
                _canonicalLogger.LogInformation("Test message");
                var entries = _canonicalLogger.FlushLogs().ToList();
                Assert.Single(entries);
            }
        }

        [Fact]
        public void Logger_HandlesStructuredLogging()
        {
            _canonicalLogger.LogInformation("Order {OrderId} processed for {Customer}", 123, "Test");
            
            var entries = _canonicalLogger.FlushLogs().ToList();
            Assert.Single(entries);
            Assert.Contains("OrderId", entries[0].State.Keys);
            Assert.Contains("Customer", entries[0].State.Keys);
            Assert.Equal(123, entries[0].State["OrderId"]);
            Assert.Equal("Test", entries[0].State["Customer"]);
        }
    }

    public class CanonicalLoggerProviderTests
    {
        private readonly ILoggerFactory _factory;
        private readonly CanonicalLoggerProvider _provider;

        public CanonicalLoggerProviderTests()
        {
            _factory = LoggerFactory.Create(builder => builder.AddConsole());
            _provider = new CanonicalLoggerProvider(_factory);
        }

        [Fact]
        public void Provider_CreatesSingleInstancePerCategory()
        {
            var logger1 = _provider.CreateLogger("Category1");
            var logger2 = _provider.CreateLogger("Category1");
            var logger3 = _provider.CreateLogger("Category2");

            Assert.Same(logger1, logger2);
            Assert.NotSame(logger1, logger3);
        }

        [Fact]
        public void Provider_FlushesAllLoggers()
        {
            var logger1 = _provider.CreateLogger("Category1") as CanonicalLogger;
            var logger2 = _provider.CreateLogger("Category2") as CanonicalLogger;

            logger1!.LogInformation("Message 1");
            logger2!.LogInformation("Message 2");

            var allLogs = _provider.FlushAllLogs().ToList();
            Assert.Equal(2, allLogs.Count);
        }
    }

    public class CanonicalLoggingMiddlewareTests
    {
        [Fact]
        public async Task Middleware_FlushesLogsAfterRequest()
        {
            var factory = LoggerFactory.Create(builder => builder.AddConsole());
            var provider = new CanonicalLoggerProvider(factory);
            var middleware = new CanonicalLoggingMiddleware(
                async (context) => {
                    var logger = provider.CreateLogger("Test") as CanonicalLogger;
                    logger!.LogInformation("Test message");
                    await Task.CompletedTask;
                },
                provider);

            var context = new DefaultHttpContext();
            await middleware.InvokeAsync(context);

            var logs = provider.FlushAllLogs().ToList();
            Assert.Single(logs);
            Assert.Contains("Test message", logs[0].Message);
        }
    }

    public class CanonicalLoggingExtensionsTests
    {
        [Fact]
        public void AddCanonicalLogging_RegistersServices()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddCanonicalLogging();
            
            var serviceProvider = services.BuildServiceProvider();
            var provider = serviceProvider.GetService<CanonicalLoggerProvider>();
            Assert.NotNull(provider);
        }
    }
}
