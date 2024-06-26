﻿// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch.Mapping;
using ElasticsearchCodeSearch.Models;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Logging;
using Elastic.Transport.Products.Elasticsearch;

namespace ElasticsearchCodeSearch.Shared.Elasticsearch
{
    public class ElasticCodeSearchClient
    {
        private readonly ILogger<ElasticCodeSearchClient> _logger;

        private readonly ElasticsearchClient _client;
        private readonly string _indexName;

        public ElasticCodeSearchClient(ILogger<ElasticCodeSearchClient> logger, IOptions<ElasticCodeSearchOptions> options)
        {
            _logger = logger;
            _indexName = options.Value.IndexName;
            _client = CreateClient(options.Value);
        }

        public virtual ElasticsearchClient CreateClient(ElasticCodeSearchOptions options)
        {
            var settings = new ElasticsearchClientSettings(new Uri(options.Uri))
                .CertificateFingerprint(options.CertificateFingerprint)
                .Authentication(new BasicAuthentication(options.Username, options.Password));

            return new ElasticsearchClient(settings);
        }

        public async Task<Elastic.Clients.Elasticsearch.IndexManagement.ExistsResponse> IndexExistsAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var indexExistsResponse = await _client.Indices
                .ExistsAsync(_indexName, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("ExistsResponse DebugInformation: {DebugInformation}", indexExistsResponse.DebugInformation);
            }

            return indexExistsResponse;
        }

        public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var pingResponse = await _client
                .PingAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Ping DebugInformation: {DebugInformation}", pingResponse.DebugInformation);
            }

            return pingResponse;
        }

        public async Task<DeleteByQueryResponse> DeleteByOwnerAsync(string owner, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // We have three fields, that must match.
            var boolQuery = new BoolQuery
            {
                Must = new Query[]
                {
                    new TermQuery(Infer.Field<CodeSearchDocument>(f => f.Owner)) { Value = owner, CaseInsensitive = true }
                }
            };

            var deleteByQueryResponse = await _client
                .DeleteByQueryAsync<CodeSearchDocument>(_indexName, request => request.Query(boolQuery), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteByQueryResponse DebugInformation: {DebugInformation}", deleteByQueryResponse.DebugInformation);
            }

            return deleteByQueryResponse;
        }

        public async Task<DeleteByQueryResponse> DeleteByOwnerRepositoryAndBranchAsync(string owner, string repository, string branch, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // We have three fields, that must match.
            var boolQuery = new BoolQuery
            {
                Must = new Query[]
                {
                    new TermQuery(Infer.Field<CodeSearchDocument>(f => f.Owner)) { Value = owner, CaseInsensitive = true },
                    new TermQuery(Infer.Field<CodeSearchDocument>(f => f.Repository)) { Value = repository, CaseInsensitive = true },
                    new TermQuery(Infer.Field<CodeSearchDocument>(f => f.Branch)) { Value = branch, CaseInsensitive = true },
                }
            };

            var deleteByQueryResponse = await _client
                .DeleteByQueryAsync<CodeSearchDocument>(_indexName, request => request.Query(boolQuery), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteByQueryResponse DebugInformation: {DebugInformation}", deleteByQueryResponse.DebugInformation);
            }

            return deleteByQueryResponse;
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var createIndexResponse = await _client.Indices.CreateAsync(_indexName, descriptor => descriptor
                .Settings(settings => settings
                    .Codec("best_compression")
                    .Analysis(analysis => analysis
                        .Normalizers(normalizers => normalizers
                            .Custom("sha_normalizer", normalizer => normalizer
                                .Filter(new[] { "lowercase" })
                            )
                        )
                        .Analyzers(analyzers => analyzers
                            .Custom("default", custom => custom
                                .Tokenizer("standard").Filter(new[]
                                {
                                    "lowercase",
                                    "stemmer"
                                })
                             )
                            .Custom("whitespace_reverse", custom => custom
                                .Tokenizer("whitespace").Filter(new[]
                                {
                                    "lowercase",
                                    "asciifolding",
                                    "reverse"
                                })
                            )
                            .Custom("code_analyzer", custom => custom
                                .Tokenizer("whitespace").Filter(new[]
                                {
                                    "word_delimiter_graph_filter",
                                    "flatten_graph",
                                    "lowercase",
                                    "asciifolding",
                                    "remove_duplicates"
                                })
                            )
                            .Custom("custom_path_tree", custom => custom
                                .Tokenizer("custom_hierarchy")
                            )
                            .Custom("custom_path_tree_reversed", custom => custom
                                .Tokenizer("custom_hierarchy_reversed")
                            )
                        )
                        .Tokenizers(tokenizers => tokenizers
                            .PathHierarchy("custom_hierarchy", tokenizer => tokenizer
                                .Delimiter("/"))
                            .PathHierarchy("custom_hierarchy_reversed", tokenizer => tokenizer
                                .Reverse(true).Delimiter("/"))
                        )
                        .TokenFilters(filters => filters
                            .WordDelimiterGraph("word_delimiter_graph_filter", filter => filter
                                .PreserveOriginal(true)
                            )
                        )
                    )
                 )
                .Mappings(mapping => mapping
                        .Properties<CodeSearchDocument>(properties => properties
                            .Keyword(properties => properties.Id, keyword => keyword
                                .IndexOptions(IndexOptions.Docs)
                                .Normalizer("sha_normalizer")
                             )
                            .Keyword(properties => properties.Owner)
                            .Keyword(properties => properties.Repository)
                            .Text(properties => properties.Path, text => text
                                .Fields(fields => fields
                                    .Text("tree", tree => tree.Analyzer("custom_path_tree"))
                                    .Text("tree_reversed", tree_reversed => tree_reversed.Analyzer("custom_path_tree_reversed"))
                                )
                            )
                            .Text(properties => properties.Filename, text => text
                                .Analyzer("code_analyzer")
                                .Store(true)
                                .Fields(fields => fields
                                    .Text("reverse", tree => tree.Analyzer("whitespace_reverse"))
                                )
                             )
                            .Keyword(properties => properties.CommitHash, keyword => keyword
                                .IndexOptions(IndexOptions.Docs)
                                .Normalizer("sha_normalizer")
                             )
                            .Text(properties => properties.Content, text => text
                                .IndexOptions(IndexOptions.Positions)
                                .Analyzer("code_analyzer")
                                .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
                                .Store(true))
                            .Keyword(properties => properties.Branch)
                            .Keyword(properties => properties.Permalink)
                            .Date(properties => properties.LatestCommitDate))), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("CreateIndexResponse DebugInformation: {DebugInformation}", createIndexResponse.DebugInformation);
            }

            return createIndexResponse;
        }

        public async Task<DeleteByQueryResponse> DeleteAllAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteByQueryResponse = await _client
                .DeleteByQueryAsync<CodeSearchDocument>(_indexName, request => request.Query(query => query.MatchAll(x => { })), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteResponse DebugInformation: {DebugInformation}", deleteByQueryResponse.DebugInformation);
            }

            return deleteByQueryResponse;
        }

        public async Task<DeleteIndexResponse> DeleteIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteIndexResponse = await _client.Indices.DeleteAsync(_indexName).ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteIndexResponse DebugInformation: {DebugInformation}", deleteIndexResponse.DebugInformation);
            }

            return deleteIndexResponse;
        }

        public async Task<IndicesStatsResponse> GetSearchStatistics(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var indicesStatResponse = await _client
                .Indices.StatsAsync(request => request.Indices(_indexName))
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("IndicesStatsResponse DebugInformation: {DebugInformation}", indicesStatResponse.DebugInformation);
            }

            return indicesStatResponse;
        }

        public async Task<DeleteResponse> DeleteAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteResponse = await _client
                .DeleteAsync<CodeSearchDocument>(documentId, x => x.Index(_indexName), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteResponse DebugInformation: {DebugInformation}", deleteResponse.DebugInformation);
            }

            return deleteResponse;
        }

        public async Task<GetResponse<CodeSearchDocument>> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var getResponse = await _client
                .GetAsync<CodeSearchDocument>(documentId, x => x.Index(_indexName), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("GetResponse DebugInformation: {DebugInformation}", getResponse.DebugInformation);
            }

            return getResponse;
        }

        public async Task<HealthResponse> WaitForClusterAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var healthRequest = new HealthRequest()
            {
                Timeout = timeout
            };

            var clusterHealthResponse = await _client.Cluster
                .HealthAsync(healthRequest, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("ClusterHealthResponse DebugInformation: {DebugInformation}", clusterHealthResponse.DebugInformation);
            }

            return clusterHealthResponse;
        }

        public async Task<BulkResponse> BulkIndexAsync(IEnumerable<CodeSearchDocument> documents, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var bulkResponse = await _client.BulkAsync(b => b
                .Index(_indexName)
                .IndexMany(documents), cancellationToken)
                .ConfigureAwait(false);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("BulkResponse DebugInformation: {DebugInformation}", bulkResponse.DebugInformation);
            }

            return bulkResponse;
        }

        public Task<SearchResponse<CodeSearchDocument>> SearchAsync(CodeSearchRequest searchRequest, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            // Convert to Elasticsearch Sort Fields
            var sortOptionsArray = searchRequest.Sort
                .Select(sortField => ConvertToSortOptions(sortField))
                .ToArray();

            // Build the Search Query:
            var codeSearchDocuments = _client.SearchAsync<CodeSearchDocument>(searchRequestDescriptor => searchRequestDescriptor
                // Query this Index:
                .Index(_indexName)
                // Setup Pagination:
                .From(searchRequest.From).Size(searchRequest.Size)
                // Setup the QueryString:
                .Query(q => q
                    .QueryString(new QueryStringQuery()
                    {
                        AllowLeadingWildcard = true,
                        Query = searchRequest.Query,
                    })
                )
                // Setup the Highlighters:
                .Highlight(highlight => highlight
                    .Fields(fields => fields
                        .Add(Infer.Field<CodeSearchDocument>(f => f.Content), hf => hf
                            .Fragmenter(HighlighterFragmenter.Span)
                            .PreTags(new[] { ElasticsearchConstants.HighlightStartTag })
                            .PostTags(new[] { ElasticsearchConstants.HighlightEndTag })
                            .NumberOfFragments(0)
                        )
                        .Add(Infer.Field<CodeSearchDocument>(f => f.Filename), hf => hf
                            .Fragmenter(HighlighterFragmenter.Span)
                            .PreTags(new[] { ElasticsearchConstants.HighlightStartTag })
                            .PostTags(new[] { ElasticsearchConstants.HighlightEndTag })
                            .NumberOfFragments(0)
                        )
                    )
                )
                // Setup the Search Order:
                .Sort(sortOptionsArray), cancellationToken);

            return codeSearchDocuments;
        }

        private static SortOptions ConvertToSortOptions(SortField sortField)
        {
            var sortOrder = sortField.Order == SortOrderEnum.Ascending ? SortOrder.Asc : SortOrder.Desc;

            return SortOptions.Field(new Field(sortField.Field), new FieldSort { Order = sortOrder });
        }
    }
}