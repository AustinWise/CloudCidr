using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Austin.CloudCidr;

public class CloudRegionLocator : ICloudRegionLocator
{
    private readonly Task<CloudRegionData> _dataLoading;
    private CloudRegionData? _data;

    public CloudRegionLocator()
    {
        _dataLoading = Task.Run(loadInitalData);
    }

    private async Task<CloudRegionData> loadInitalData()
    {
        var gdata = await JsonLoading.LoadGoogleData();
        var ipv4Keys = new List<uint>();
        var ipv4Values = new List<CloudRegion>();
        foreach (var (ip, prefixLen, region) in gdata)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                if (prefixLen <= 0 || prefixLen > 32)
                    throw new Exception();

                uint address = 0;
                int bytesWritten;
                if (!ip.TryWriteBytes(MemoryMarshal.AsBytes(new Span<uint>(ref address)), out bytesWritten))
                {
                    throw new Exception();
                }
                Debug.Assert(bytesWritten == 4);

                if (BitConverter.IsLittleEndian)
                {
                    address = BinaryPrimitives.ReverseEndianness(address);
                }

                ipv4Keys.Add(address);
                ipv4Values.Add(new CloudRegion(CloudProvider.GoogleCloud, region, prefixLen));
            }
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                // TODO
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        var ipv4KeyArray = ipv4Keys.ToArray();
        var ipv4ValueArray = ipv4Values.ToArray();

        Array.Sort(ipv4KeyArray, ipv4ValueArray);

        // TODO: validate that data doess not overlap

        return new CloudRegionData(ipv4KeyArray, ipv4ValueArray);
    }

    private async ValueTask<CloudRegion> WaitForDataAndLookup(IPAddress address)
    {
        var data = _data ??= await _dataLoading;
        return data.Lookup(address);
    }

    public ValueTask<CloudRegion> Lookup(IPAddress address)
    {
        var data = _data;
        if (data is null)
        {
            return WaitForDataAndLookup(address);
        }
        else
        {
            return ValueTask.FromResult(data.Lookup(address));
        }
    }

    class CloudRegionData
    {
        private readonly uint[] _ipv4Key;
        private readonly CloudRegion[] _ipv4Value;

        public CloudRegionData(uint[] ipv4Key, CloudRegion[] ipv4Value)
        {
            this._ipv4Key = ipv4Key;
            this._ipv4Value = ipv4Value;
        }

        public CloudRegion Lookup(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                uint key = 0;
                int bytesWritten;
                if (!address.TryWriteBytes(MemoryMarshal.AsBytes(new Span<uint>(ref key)), out bytesWritten))
                {
                    throw new Exception();
                }
                Debug.Assert(bytesWritten == 4);

                if (BitConverter.IsLittleEndian)
                {
                    key = BinaryPrimitives.ReverseEndianness(key);
                }

                int pos = Array.BinarySearch(_ipv4Key, key);

                if (pos >= 0)
                {
                    // exact match
                    Debug.Assert(key == _ipv4Key[pos]);
                    return _ipv4Value[pos];
                }
                else
                {
                    // If we did not find an exact match, ~pos is pointing at the item in the array that is larger.
                    pos = ~pos;
                    if (pos == 0)
                    {
                        // all the addresses are greater, so there is no match
                        return default;
                    }

                    // adjust to point to the item that is one smaller than us
                    pos -= 1;
                }

                var region = _ipv4Value[pos];
                uint mask = uint.MaxValue << (32 - region._prefixLength);
                if ((key & mask) == _ipv4Key[pos])
                {
                    return region;
                }
                else
                {
                    return default;
                }
            }
            else if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
