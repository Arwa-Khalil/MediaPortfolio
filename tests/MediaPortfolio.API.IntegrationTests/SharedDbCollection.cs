using Xunit;

namespace MediaPortfolio.API.IntegrationTests;

[CollectionDefinition("SharedDbCollection")]
public class SharedDbCollection : ICollectionFixture<ApiWebApplicationFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
