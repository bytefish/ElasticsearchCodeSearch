using Microsoft.AspNetCore.Components;

namespace ElasticsearchCodeSearch.Client.Extensions
{
    public static class StringExtensions
    {
        public static MarkupString? AsMarkupString(this string? source)
        {
            if(source == null)
            {
                return null;
            }

            return (MarkupString?)source;
        }
    }
}
