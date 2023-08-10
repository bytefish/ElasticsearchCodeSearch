// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch.IndexManagement;
using ElasticsearchCodeSearch.Shared.Dto;

namespace ElasticsearchCodeSearch.Converters
{
    public static class CodeSearchStatisticsConverter
    {
        public static CodeSearchStatisticsDto Convert(IndicesStatsResponse indicesStatsResponse)
        {
            if(indicesStatsResponse.Indices == null)
            {
                throw new Exception("No statistics available");
            }

            if(indicesStatsResponse.Indices.Count != 1)
            {
                throw new Exception($"Expected '1' Index, but got '{indicesStatsResponse.Indices.Count}'");
            }

            var indexName = indicesStatsResponse.Indices.First().Key;
            var indexStats = indicesStatsResponse.Indices.First().Value;

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
