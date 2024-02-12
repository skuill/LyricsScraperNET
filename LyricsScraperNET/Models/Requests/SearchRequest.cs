using Microsoft.VisualBasic;

namespace LyricsScraperNET.Models.Requests
{
    public abstract class SearchRequest
    {
        public abstract bool IsValid(out string error);
    }
}
