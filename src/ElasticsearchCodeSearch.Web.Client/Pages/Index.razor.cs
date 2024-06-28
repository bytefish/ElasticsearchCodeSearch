// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Web.Client.Infrastructure;
using ElasticsearchCodeSearch.Web.Client.Localization;
using ElasticsearchCodeSearch.Web.Client.Models;
using ElasticsearchCodeSearch.Shared.Dto;
using ElasticsearchCodeSearch.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace ElasticsearchCodeSearch.Web.Client.Pages
{
    public partial class Index
    {
        /// <summary>
        /// Elasticsearch Search Client.
        /// </summary>
        [Inject]
        public ElasticsearchCodeSearchService ElasticsearchCodeSearchService { get; set; } = default!;

        /// <summary>
        /// Shared String Localizer.
        /// </summary>
        [Inject]
        public IStringLocalizer<SharedResource> Loc { get; set; } = default!;

        /// <summary>
        /// Search Statistics.
        /// </summary>
        private List<ElasticsearchIndexMetrics> _elasticsearchIndexMetrics = new List<ElasticsearchIndexMetrics>();

        protected override async Task OnInitializedAsync()
        {
            var codeSearchStatistics = await ElasticsearchCodeSearchService.SearchStatisticsAsync(default);

            _elasticsearchIndexMetrics = ConvertToElasticsearchIndexMetric(codeSearchStatistics);
        }

        private List<ElasticsearchIndexMetrics> ConvertToElasticsearchIndexMetric(List<CodeSearchStatisticsDto>? codeSearchStatistics)
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

        private List<ElasticsearchMetric> ConvertToElasticsearchMetrics(CodeSearchStatisticsDto codeSearchStatistic)
        {
            return new List<ElasticsearchMetric>()
            {
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_IndexName"],
                    Key = "indices[i]",
                    Value = codeSearchStatistic.IndexName
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_IndexSize"],
                    Key = "indices.store.size_in_bytes",
                    Value = DataSizeUtils.TotalMegabytesString(codeSearchStatistic.IndexSizeInBytes ?? 0)
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_TotalNumberOfDocumentsIndexed"],
                    Key = "indices.docs.count",
                    Value = codeSearchStatistic.TotalNumberOfDocumentsIndexed?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_NumberOfDocumentsCurrentlyBeingIndexed"],
                    Key = "indices.indexing.index_current",
                    Value = codeSearchStatistic.NumberOfDocumentsCurrentlyBeingIndexed?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_TotalTimeSpentIndexingDocuments"],
                    Key = "indices.indexing.index_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentIndexingDocumentsInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_TotalTimeSpentBulkIndexingDocuments"],
                    Key = "indices.bulk.total_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentBulkIndexingDocumentsInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_TotalNumberOfQueries"],
                    Key = "indices.search.query_total",
                    Value = codeSearchStatistic.TotalNumberOfQueries?.ToString()
                },

                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_TotalTimeSpentOnQueries"],
                    Key = "indices.search.query_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentOnQueriesInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_NumberOfQueriesCurrentlyInProgress"],
                    Key = "indices.search.query_current",
                    Value = codeSearchStatistic.NumberOfQueriesCurrentlyInProgress?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_TotalNumberOfFetches"],
                    Key = "indices.search.fetch_total",
                    Value = codeSearchStatistic.TotalNumberOfFetches?.ToString()
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_TotalTimeSpentOnFetches"],
                    Key = "indices.search.fetch_time_in_millis",
                    Value = TimeFormattingUtils.MillisecondsToSeconds(codeSearchStatistic.TotalTimeSpentOnFetchesInMilliseconds, string.Empty)
                },
                new ElasticsearchMetric
                {
                    Name = Loc["Metrics_NumberOfFetchesCurrentlyInProgress"],
                    Key = "indices.search.fetch_current",
                    Value = codeSearchStatistic.NumberOfFetchesCurrentlyInProgress?.ToString()
                },
            };
        }
    }
}
