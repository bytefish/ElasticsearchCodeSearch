// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch.IndexManagement;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Converters
{
    public static class CodeSearchStatisticsConverter
    {
        public static List<CodeSearchStatisticsDto> Convert(IndicesStatsResponse indicesStatsResponse)
        {
            if (indicesStatsResponse.Indices == null)
            {
                throw new Exception("No statistics available");
            }

            return indicesStatsResponse.Indices
                .Select(x => Convert(x.Key, x.Value))
                .ToList();
        }

        public static CodeSearchStatisticsDto Convert(string indexName, IndicesStats indexStats)
        {
            return new CodeSearchStatisticsDto
            {
                IndexName = indexName,
                IndexSizeInBytes = indexStats.Total?.Store?.SizeInBytes,
                TotalNumberOfDocumentsIndexed = indexStats.Total?.Indexing?.IndexTotal,
                NumberOfDocumentsCurrentlyBeingIndexed = indexStats.Total?.Indexing?.IndexCurrent,
                TotalNumberOfFetches = indexStats.Total?.Search?.FetchTotal,
                NumberOfFetchesCurrentlyInProgress = indexStats.Total?.Search?.FetchCurrent,
                TotalNumberOfQueries = indexStats.Total?.Search?.QueryTotal,
                NumberOfQueriesCurrentlyInProgress = indexStats.Total?.Search?.QueryCurrent,
                TotalTimeSpentIndexingDocumentsInMilliseconds = indexStats.Total?.Indexing?.IndexTimeInMillis,
                TotalTimeSpentOnFetchesInMilliseconds = indexStats.Total?.Search?.FetchTimeInMillis,
                TotalTimeSpentOnQueriesInMilliseconds = indexStats.Total?.Search?.QueryTimeInMillis,
            };
        }
    }
}
