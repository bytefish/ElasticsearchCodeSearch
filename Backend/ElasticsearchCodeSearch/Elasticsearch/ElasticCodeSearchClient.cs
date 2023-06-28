// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Cluster;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.Analysis;
using ElasticsearchCodeSearch.Options;
using ElasticsearchCodeSearch.Logging;
using ElasticsearchCodeSearch.Elasticsearch.Model;

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
                .Settings(settings => settings.
                    Analysis(analysis => analysis
                        .Analyzers(analyzers => analyzers
                            .Custom("code_analyzer", custom => custom
                                .Tokenizer("whitespace")
                                .Filter(new[]
                                {
                                    "word_delimiter_graph_filter",
                                    "flatten_graph",
                                    "lowercase",
                                    "asciifolding",
                                    "remove_duplicates"
                                })
                            )
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
                            .Text(properties => properties.Id)
                            .Text(properties => properties.Owner, x => x
                                .Analyzer("code_analyzer")
                                .TermVector(TermVectorOption.WithPositionsOffsetsPayloads)
                                .Store(true))
                            .Text(properties => properties.Repository)
                            .Text(properties => properties.Filename)
                            .Text(properties => properties.Content)
                            .Date(properties => properties.LatestCommitDate))), cancellationToken);

            _logger.LogDebug("CreateIndexResponse DebugInformation: {DebugInformation}", createIndexResponse.DebugInformation);

            return createIndexResponse;
        }

        public async Task<DeleteResponse> DeleteAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var deleteResponse = await _client.DeleteAsync<CodeSearchDocument>(documentId, x => x.Index(_indexName), cancellationToken);

            _logger.LogDebug("DeleteResponse DebugInformation: {DebugInformation}", deleteResponse.DebugInformation);

            return deleteResponse;
        }

        public async Task<GetResponse<CodeSearchDocument>> GetDocumentByIdAsync(string documentId, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var getResponse = await _client.GetAsync<CodeSearchDocument>(documentId, x => x.Index(_indexName), cancellationToken);

            _logger.LogDebug("GetResponse DebugInformation: {DebugInformation}", getResponse.DebugInformation);

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

            _logger.LogDebug("ClusterHealthResponse DebugInformation: {DebugInformation}", clusterHealthResponse.DebugInformation);

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

        public async Task<IndexResponse> IndexAsync(CodeSearchDocument document, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var indexResponse = await _client.IndexAsync(document, x => x
                .Id(document.Id)
                .Index(_indexName), cancellationToken);

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("IndexResponse DebugInformation: {DebugInformation}", indexResponse.DebugInformation);
            }

            return indexResponse;
        }

        public Task<SearchResponse<CodeSearchDocument>> SearchAsync(string query, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            return _client.SearchAsync<CodeSearchDocument>(searchRequestDescriptor => searchRequestDescriptor
                // Query this Index:
                .Index(_indexName)
                // Setup the Query:
                .Query(q => q.MultiMatch(mm => mm
                    .Query(query)
                    .Type(TextQueryType.BoolPrefix)
                    .Fields(new[]
                    {
                        Infer.Field<CodeSearchDocument>(d => d.Content),
                        Infer.Field<CodeSearchDocument>(d => d.Filename)
                    }))
                )
                // Setup the Highlighters:
                .Highlight(highlight => highlight
                    .Fields(fields => fields
                        .Add(Infer.Field<CodeSearchDocument>(f => f.Content), new HighlightField
                        {
                            Fragmenter = HighlighterFragmenter.Span,
                            PreTags = new[] { "<strong>" },
                            PostTags = new[] { "</strong>" },
                            FragmentSize = 150,
                            NoMatchSize = 150,
                            NumberOfFragments = 5,
                        })
                        .Add(Infer.Field<CodeSearchDocument>(f => f.Filename), new HighlightField
                        {
                            Fragmenter = HighlighterFragmenter.Span,
                            PreTags = new[] { "<strong>" },
                            PostTags = new[] { "</strong>" },
                            FragmentSize = 150,
                            NoMatchSize = 150,
                            NumberOfFragments = 5,
                            MaxAnalyzedOffset = 1000000
                        })
                    )
                )
                // Setup the Search Order:
                .Sort(new[]
                {
                    SortOptions.Field(Infer.Field<CodeSearchDocument>(x => x.LatestCommitDate), new FieldSort { Order = SortOrder.Desc })
                }), cancellationToken);
        }

        public virtual ElasticsearchClient CreateClient(ElasticCodeSearchOptions options)
        {
            var settings = new ElasticsearchClientSettings(new Uri(options.Uri))
                .CertificateFingerprint(options.CertificateFingerprint)
                .Authentication(new BasicAuthentication(options.Username, options.Password));

            return new ElasticsearchClient(settings);
        }
    }
}