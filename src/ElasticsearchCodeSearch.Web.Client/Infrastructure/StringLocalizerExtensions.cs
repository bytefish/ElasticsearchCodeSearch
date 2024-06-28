// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Localization;

namespace ElasticsearchCodeSearch.Web.Client.Infrastructure
{
    public static class StringLocalizerExtensions
    {
        public static string TranslateEnum<TResource, TEnum>(this IStringLocalizer<TResource> localizer, TEnum enumValue)
        {
            var key = $"{typeof(TEnum).Name}_{enumValue}";

            var res = localizer.GetString(key);

            return res;
        }
    }
}
