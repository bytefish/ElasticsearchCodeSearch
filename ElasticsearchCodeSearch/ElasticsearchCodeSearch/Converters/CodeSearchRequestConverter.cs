// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;

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
