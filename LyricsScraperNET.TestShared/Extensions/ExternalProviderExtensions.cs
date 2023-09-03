using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.UnitTest.TestModel;
using Moq;
using System;

namespace LyricsScraperNET.TestShared.Extensions
{
    public static class ExternalProviderExtensions
    {
        public static IExternalProvider ConfigureExternalProvider(this IExternalProvider externalProvider, LyricsTestData testData)
        {
            var mockWebClient = new Mock<IWebClient>();
            mockWebClient.Setup(x => x.Load(It.IsAny<Uri>())).Returns(testData.LyricPageData);

            externalProvider.WithWebClient(mockWebClient.Object);
            return externalProvider;
        }
    }
}
