// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Components;

namespace ElasticsearchCodeSearch.Web.Client.Extensions
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
