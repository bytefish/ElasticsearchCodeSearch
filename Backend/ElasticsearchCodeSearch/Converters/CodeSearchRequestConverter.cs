using ElasticsearchCodeSearch.Dto;
using ElasticsearchCodeSearch.Models;

namespace ElasticsearchCodeSearch.Converters
{
    public static class CodeSearchRequestConverter
    {
        public static CodeSearchRequest Convert(CodeSearchRequestDto source)
        {
            return new CodeSearchRequest
            {
                Query = source.Query,
                From = source.From,
                Size = source.Size,
                Sort = SortFieldConverter.Convert(source.Sort)
            };
        }
    }
}
