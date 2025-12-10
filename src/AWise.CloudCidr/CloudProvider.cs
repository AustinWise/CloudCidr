namespace Austin.CloudCidr;

// TODO: figure out a policy for how to refer to clouds.
// For example "AWS" vs "Amazon Web Services", "Azure" vs "Microsoft Azure".

/// <summary></summary>
public enum CloudProvider
{
    Unknown = 0,
    GoogleCloud = 1,
    AmazonWebServices = 2,
    MicrosoftAzure = 3,
}
