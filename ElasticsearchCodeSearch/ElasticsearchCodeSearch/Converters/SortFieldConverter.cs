// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Converters
{
    public static class SortFieldConverter
    {

        public static List<SortField> Convert(List<SortFieldDto> source)
        {
            return source
                .Select(x => Convert(x))
                .ToList();
        }
        
        public static SortField Convert(SortFieldDto source)
        {
            return new SortField
            {
                Field = source.Field,
                Order = SortOrderEnumConverter.Convert(source.Order)
            };
        }
    }
}
