using ElasticsearchCodeSearch.Client.Infrastructure;
using ElasticsearchCodeSearch.Client.Models;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace ElasticsearchCodeSearch.Client.Pages
{
    public partial class Index
    {
        /// <summary>
        /// Elasticsearch Search Client.
        /// </summary>
        [Inject]
        public ElasticsearchCodeSearchService ElasticsearchCodeSearchService { get; set; } = default!;

        /// <summary>
        /// Search Statistics.
        /// </summary>
        private List<ElasticsearchIndexMetrics> _elasticsearchIndexMetrics = new List<ElasticsearchIndexMetrics>();

        protected override async Task OnInitializedAsync()
        {
            var codeSearchStatistics = await ElasticsearchCodeSearchService.SearchStatisticsAsync(default);

            _elasticsearchIndexMetrics = ConvertToElasticsearchIndexMetric(codeSearchStatistics);
        }

        private static List<ElasticsearchIndexMetrics> ConvertToElasticsearchIndexMetric(List<CodeSearchStatisticsDto>? codeSearchStatistics)
        {
            if(codeSearchStatistics == null)
            {
                return new List<ElasticsearchIndexMetrics>();
            }

            return codeSearchStatistics
                .Select(x => new ElasticsearchIndexMetrics
                {
                    Index = x.IndexName,
                    Metrics = ConvertToElasticsearchMetrics(x)
                }).ToList();

        }

        private static List<ElasticsearchMetric> ConvertToElasticsearchMetrics(CodeSearchStatisticsDto codeSearchStatistic)
        {
            return new List<ElasticsearchMetric>()
            {
                new ElasticsearchMetric
                {
                    Name = "Index",
                    Key = "indices[i]",
                    Value = codeSearchStatistic.IndexName
                },
                new ElasticsearchMetric
                {
                    Name = "Index Size (Mb)",
                    Key = "indices.store.size_in_bytes",
                    Value = DataSizeUtils.TotalMegabytesString(codeSearchStatistic.IndexSizeInBytes ?? 0)
                },
                new ElasticsearchMetric
                {
                    Name = "Total Number of Documents Indexed",
                    Key = "indices.docs.count",
                    Value = codeSearchStatistic.TotalNumberOfDocumentsIndexed?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = "Number of Documents Currently Being Indexed",
                    Key = "indices.indexing.index_current",
                    Value = codeSearchStatistic.NumberOfDocumentsCurrentlyBeingIndexed?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = "Total Time Spent Indexing Documents",
                    Key = "indices.indexing.index_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentIndexingDocumentsInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = "Total Time Spent Bulk Indexing Documents",
                    Key = "indices.bulk.total_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentBulkIndexingDocumentsInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = "Total Number of Queries",
                    Key = "indices.search.query_total",
                    Value = codeSearchStatistic.TotalNumberOfQueries?.ToString()
                },

                new ElasticsearchMetric
                {
                    Name = "Total Time Spent on Queries",
                    Key = "indices.search.query_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentOnQueriesInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = "Number of Queries currently in Progress",
                    Key = "indices.search.query_current",
                    Value = codeSearchStatistic.NumberOfQueriesCurrentlyInProgress?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = "Total Number of Fetches",
                    Key = "indices.search.fetch_total",
                    Value = codeSearchStatistic.TotalNumberOfFetches?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = "Total Time Spent on Fetches in Seconds",
                    Key = "indices.search.fetch_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentOnFetchesInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = "Number of Fetches Currently In Progress",
                    Key = "indices.search.fetch_current",
                    Value = codeSearchStatistic.NumberOfFetchesCurrentlyInProgress?.ToString()
                },

            };
        }
    }
}
