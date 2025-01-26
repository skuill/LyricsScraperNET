using System;
using LyricsScraperNET.Providers.KPopLyrics;
using Xunit;

namespace LyricsScraperNET.UnitTest.Providers.KPopLyrics
{
    public class KPopLyricsUriConverterTests
    {
        [Theory]
        [InlineData("CA$H KiD", "CHANEL No.5", "https://www.kpoplyrics.net/cah-kid-chanel-no5.html")]
        [InlineData("OOSU:HAN (우수한)", "What's The Name OF This Song (지금 나오는 곡 제목이 뭐야)", "https://www.kpoplyrics.net/oosuhan-whats-the-name-of-this-song.html")]
        [InlineData("or& (오아랜)", "Invitation to me (나에게로의 초대)", "https://www.kpoplyrics.net/or-invitation-to-me.html")]
        [InlineData("NINE.i (나인아이)", "Back to Christmas (크리스마스처럼)", "https://www.kpoplyrics.net/ninei-back-to-christmas.html")]
        [InlineData("Lolo Zouaï & BIBI", "Galipette (BIBI Remix)", "https://www.kpoplyrics.net/lolo-zoua-bibi-galipette-bibi-remix.html")]
        [InlineData("LONG:D (롱디)", "SPACEDOG", "https://www.kpoplyrics.net/longd-spacedog.html")]
        [InlineData("ENHYPEN (엔하이픈)", "XO (Only If You Say Yes)", "https://www.kpoplyrics.net/enhypen-xo-only-if-you-say-yes.html")]
        public void GetLyricUri_MultipleInputs_ShouldBeParse(string artistName, string songName, string expectedUri)
        {
            // Arrange
            var uriConverter = new KPopLyricsUriConverter();

            // Act
            var actual = uriConverter.GetLyricUri(artistName, songName);

            // Assert
            Assert.Equal(new Uri(expectedUri), actual);
        }
    }
}