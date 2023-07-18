using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Converters
{
    public static class HighlightedContentConverter
    {
        public static List<HighlightedContentDto> Convert(List<HighlightedContent> source)
        {
            return source
                .Select(x => Convert(x))
                .ToList();
        }

        public static HighlightedContentDto Convert(HighlightedContent source)
        {
            return new HighlightedContentDto
            {
                LineNo = source.LineNo,
                Content = source.Content,
                IsHighlight = source.IsHighlight,
            };
        }
    }
}
