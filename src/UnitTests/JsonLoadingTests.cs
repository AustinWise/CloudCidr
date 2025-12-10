namespace AWise.CloudCidr.UnitTests
{
    public class JsonLoadingTests
    {
        [Fact]
        public async Task LoadGoogleData()
        {
            var data = await JsonLoading.LoadGoogleData();
            Assert.NotEmpty(data);
        }
    }
}