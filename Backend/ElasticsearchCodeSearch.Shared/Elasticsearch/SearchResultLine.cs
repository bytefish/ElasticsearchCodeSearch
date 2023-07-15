using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchCodeSearch.Shared.Elasticsearch
{
    public class SearchResultLine
    {
        public required int LineNo { get; set; }

        public required string? Data { get; set; }

        public required bool IsHighlightStart { get; set; }
    }
}
