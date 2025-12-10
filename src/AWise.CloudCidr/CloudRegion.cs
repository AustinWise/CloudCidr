namespace AWise.CloudCidr;

public readonly struct CloudRegion
{
    private readonly string _region;
    internal readonly int _prefixLength;
    public CloudProvider Provider { get; }
    public string Region => Provider == default ? throw new InvalidOperationException() : _region;

    public CloudRegion(CloudProvider provider, string region, int prefixLength)
    {
        ArgumentException.ThrowIfNullOrEmpty(region);

        this.Provider = provider;
        this._region = region;
        this._prefixLength = prefixLength;
    }
}
