// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch.Mapping;
using ElasticsearchCodeSearch.Options;
using ElasticsearchCodeSearch.Logging;
using ElasticsearchCodeSearch.Models;

namespace ElasticsearchCodeSearch.Elasticsearch
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

            var indexExistsResponse = await _client.Indices.ExistsAsync(_indexName, cancellationToken: cancellationToken);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("ExistsResponse DebugInformation: {DebugInformation}", indexExistsResponse.DebugInformation);
            }

            if (indexExistsResponse == null)
            {
                throw new Exception();
            }

            return indexExistsResponse;
        }

        public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var pingResponse = await _client.PingAsync(cancellationToken: cancellationToken);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Ping DebugInformation: {DebugInformation}", pingResponse.DebugInformation);
            }

            return pingResponse;
        }

        public async Task<CreateIndexResponse> CreateIndexAsync(CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var createIndexResponse = await _client.Indices.CreateAsync(_indexName, descriptor => descriptor
                .Settings(settings => settings
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
                            .Text(properties => properties.Filename, text => text
                                .Fields(fields => fields
                                    .Text("tree", tree => tree.Analyzer("custom_path_tree"))
                                    .Text("tree_reversed", tree_reversed => tree_reversed.Analyzer("custom_path_tree_reversed"))
                                )
                            )
                            .Text(properties => properties.Content, text => text
                                .IndexOptions(IndexOptions.Positions)
                                .Analyzer("code_analyzer")
                                .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
                                .Store(true))
                            .Keyword(properties => properties.Permalink)
                            .Date(properties => properties.LatestCommitDate))), cancellationToken);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("CreateIndexResponse DebugInformation: {DebugInformation}", createIndexResponse.DebugInformation);
            }

            return createIndexResponse;
        }

        public async Task<DeleteResponse> DeleteAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteResponse = await _client.DeleteAsync<CodeSearchDocument>(documentId, x => x.Index(_indexName), cancellationToken);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("DeleteResponse DebugInformation: {DebugInformation}", deleteResponse.DebugInformation);
            }

            return deleteResponse;
        }

        public async Task<GetResponse<CodeSearchDocument>> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var getResponse = await _client.GetAsync<CodeSearchDocument>(documentId, x => x.Index(_indexName), cancellationToken);

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
                WaitForActiveShards = "1",
                Timeout = timeout
            };

            var clusterHealthResponse = await _client.Cluster.HealthAsync(healthRequest, cancellationToken: cancellationToken);

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
                .IndexMany(documents), cancellationToken);

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
                .Select(x => SortOptions.Field(new Field(x.Field), new FieldSort { Order = SortOrder.Desc }))
                .ToArray();
            
            // Build the Search Query:
            return _client.SearchAsync<CodeSearchDocument>(searchRequestDescriptor => searchRequestDescriptor
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
                        .Add(Infer.Field<CodeSearchDocument>(f => f.Content), new HighlightField
                        {
                            Fragmenter = HighlighterFragmenter.Span,
                            PreTags = new[] { "<strong>" },
                            PostTags = new[] { "</strong>" },
                            FragmentSize = 300,
                            NoMatchSize = 300,
                            NumberOfFragments = 5,
                        })
                        .Add(Infer.Field<CodeSearchDocument>(f => f.Filename), new HighlightField
                        {
                            Fragmenter = HighlighterFragmenter.Span,
                            PreTags = new[] { "<strong>" },
                            PostTags = new[] { "</strong>" },
                            FragmentSize = 300,
                            NoMatchSize = 300,
                            NumberOfFragments = 5
                        })
                    )
                )
                // Setup the Search Order:
                .Sort(sortOptionsArray), cancellationToken);
        }
    }
}