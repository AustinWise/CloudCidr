using System.Net;

namespace Austin.CloudCidr.Tests
{
    public class CloudRegionLocatorTests
    {
        [Fact]
        public async Task TestGoogleCloud()
        {
            var locator = new CloudRegionLocator();
            Assert.Equal(CloudProvider.Unknown, (await locator.Lookup(IPAddress.Parse("10.1.1.1"))).Provider);

            CloudRegion region;

            region = await locator.Lookup(IPAddress.Parse("34.80.0.0"));
            Assert.Equal(CloudProvider.GoogleCloud, region.Provider);
            Assert.Equal("asia-east1", region.Region);

            region = await locator.Lookup(IPAddress.Parse("34.80.0.1"));
            Assert.Equal(CloudProvider.GoogleCloud, region.Provider);
            Assert.Equal("asia-east1", region.Region);

            region = await locator.Lookup(IPAddress.Parse("34.81.0.1"));
            Assert.Equal(CloudProvider.GoogleCloud, region.Provider);
            Assert.Equal("asia-east1", region.Region);

            region = await locator.Lookup(IPAddress.Parse("34.81.255.255"));
            Assert.Equal(CloudProvider.GoogleCloud, region.Provider);
            Assert.Equal("asia-east1", region.Region);

            region = await locator.Lookup(IPAddress.Parse("34.82.0.0"));
            Assert.Equal(CloudProvider.GoogleCloud, region.Provider);
            Assert.Equal("us-west1", region.Region);

            region = await locator.Lookup(IPAddress.Parse("34.82.0.1"));
            Assert.Equal(CloudProvider.GoogleCloud, region.Provider);
            Assert.Equal("us-west1", region.Region);
        }
    }
}
