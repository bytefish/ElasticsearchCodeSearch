using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Converters
{
    public static class SortOrderEnumConverter
    {
        public static SortOrderEnum Convert(SortOrderEnumDto source)
        {
            return source switch
            {
                SortOrderEnumDto.Asc => SortOrderEnum.Ascending,
                SortOrderEnumDto.Desc => SortOrderEnum.Descending,
                _ => throw new ArgumentException($"Cannot convert from '{source}'"),
            };
        }
    }
}
