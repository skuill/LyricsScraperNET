namespace LyricsScraperNET.Helpers
{
    /// <summary>
    ///   Check input parameters
    /// </summary>
    internal static class Ensure
    {
        /// <summary>
        ///   Checks an argument if it isn't null.
        /// </summary>
        /// <param name = "value">The argument value to check</param>
        /// <param name = "name">The name of the argument</param>
        public static void ArgumentNotNull(object value, string name)
        {
            if (value != null)
            {
                return;
            }

            throw new ArgumentNullException(name);
        }

        /// <summary>
        ///   Checks an argument if it isn't null or an empty string
        /// </summary>
        /// <param name = "value">The argument value to check</param>
        /// <param name = "name">The name of the argument</param>
        public static void ArgumentNotNullOrEmptyString(string value, string name)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return;
            }

            throw new ArgumentException("String is empty or null", name);
        }

        /// <summary>
        ///   Checks an argument list if it isn't null or an empty string
        /// </summary>
        /// <param name = "value">The argument value to check</param>
        /// <param name = "name">The name of the argument</param>
        public static void ArgumentNotNullOrEmptyList<T>(IEnumerable<T> value, string name)
        {
            if (value != null && value.Any())
            {
                return;
            }

            throw new ArgumentException("List is empty or null", name);
        }
    }
}
