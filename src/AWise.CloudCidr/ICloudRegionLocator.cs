using System.Net;

namespace Austin.CloudCidr;

public interface ICloudRegionLocator
{
    ValueTask<CloudRegion> Lookup(IPAddress address);
}
