// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;

namespace ElasticsearchCodeSearch.Web.Client.Infrastructure
{
    public static class TimeFormattingUtils
    {
        public static string MillisecondsToSeconds(long? milliseconds, string defaultValue)
        {
            if(!milliseconds.HasValue)
            {
                return defaultValue;
            }

            var timeSpan = TimeSpan.FromMilliseconds(milliseconds.Value);

            return timeSpan.TotalSeconds.ToString("F");
        }

        public static string MillisecondsToSeconds(long? milliseconds, string defaultValue, CultureInfo cultureInfo)
        {
            if (!milliseconds.HasValue)
            {
                return defaultValue;
            }

            var timeSpan = TimeSpan.FromMilliseconds(milliseconds.Value);

            return timeSpan.TotalSeconds.ToString("F", cultureInfo);
        }
    }
}
