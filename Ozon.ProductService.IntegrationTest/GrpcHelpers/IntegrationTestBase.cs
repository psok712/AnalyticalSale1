using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Ozon.ProductService.Api;
using Xunit.Abstractions;

namespace Ozon.ProductService.IntegrationTest.GrpcHelpers;

public class IntegrationTestBase : IClassFixture<GrpcTestFixture<Startup>>, IDisposable
{
    private GrpcChannel? _channel;
    private readonly IDisposable? _testContext;

    protected GrpcTestFixture<Startup> Fixture { get; set; }

    private ILoggerFactory LoggerFactory => Fixture.LoggerFactory;

    protected GrpcChannel Channel => _channel ??= CreateChannel();

    private GrpcChannel CreateChannel()
    {
        return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            LoggerFactory = LoggerFactory,
            HttpHandler = Fixture.Handler
        });
    }

    protected IntegrationTestBase(GrpcTestFixture<Startup> fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;
        _testContext = Fixture.GetTestContext(outputHelper);
    }

    public void Dispose()
    {
        _testContext?.Dispose();
        _channel = null;
    }
}