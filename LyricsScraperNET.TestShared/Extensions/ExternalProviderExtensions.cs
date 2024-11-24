using FakeItEasy;
using LyricsScraperNET.Network.Abstract;
using LyricsScraperNET.Providers.Abstract;
using LyricsScraperNET.UnitTest.TestModel;
using System;

namespace LyricsScraperNET.TestShared.Extensions
{
    public static class ExternalProviderExtensions
    {
        public static IExternalProvider ConfigureExternalProvider(this IExternalProvider externalProvider, LyricsTestData testData)
        {
            var mockWebClient = A.Fake<IWebClient>();
            A.CallTo(() => mockWebClient.Load(A<Uri>._)).Returns(testData.LyricPageData);

            externalProvider.WithWebClient(mockWebClient);
            return externalProvider;
        }
    }
}
