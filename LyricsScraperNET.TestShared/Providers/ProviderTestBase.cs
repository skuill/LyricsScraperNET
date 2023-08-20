using LyricsScraperNET.TestShared.Utils;
using LyricsScraperNET.UnitTest.TestModel;
using System;
using System.Collections.Generic;

namespace LyricsScraperNET.TestShared.Providers
{
    public class ProviderTestBase
    {
        public static IEnumerable<object[]> GetTestData(string testDataPath)
        {
            if (testDataPath == null)
            {
                throw new ArgumentNullException(nameof(testDataPath));
            }
            foreach (var testData in Serializer.Deseialize<List<LyricsTestData>>(testDataPath))
            {
                yield return new object[] { testData };
            }
        }
    }
}
