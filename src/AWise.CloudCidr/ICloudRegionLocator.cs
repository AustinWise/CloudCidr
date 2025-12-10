using System.Net;

namespace AWise.CloudCidr;

public interface ICloudRegionLocator
{
    ValueTask<CloudRegion> Lookup(IPAddress address);
}
