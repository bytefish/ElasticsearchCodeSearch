﻿using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Converters
{
    public static class CodeSearchDocumentConverter
    {
        public static List<CodeSearchDocument> Convert(List<CodeSearchDocumentDto> source)
        {
            return source
                .Select(x => Convert(x))
                .ToList();
        }

        public static CodeSearchDocument Convert(CodeSearchDocumentDto source)
        {
            return new CodeSearchDocument
            {
                Id = source.Id,
                Owner = source.Owner,
                Repository = source.Repository,
                Filename = source.Filename,
                Path = source.Path,
                Content = source.Content,
                Permalink = source.Permalink,
                LatestCommitDate = source.LatestCommitDate,
            };
        }
    }
}
