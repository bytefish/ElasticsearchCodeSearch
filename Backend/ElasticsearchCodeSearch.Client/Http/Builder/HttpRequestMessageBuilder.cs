// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace ElasticsearchCodeSearch.Client.Http.Builder
{
    public class HttpRequestMessageBuilder
    {
        private class Header
        {
            public readonly string Name;
            public readonly string Value;

            public Header(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        private string url;
        private HttpMethod httpMethod;
        private IDictionary<string, string> parameters;
        private IList<Header> headers;
        private IList<UrlSegment> segments;
        private HttpContent? content;

        public HttpRequestMessageBuilder(string url, HttpMethod httpMethod)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            this.url = url;
            this.httpMethod = httpMethod;
            headers = new List<Header>();
            segments = new List<UrlSegment>();
            content = null;
            parameters = new Dictionary<string, string>();
        }

        public HttpRequestMessageBuilder HttpMethod(HttpMethod httpMethod)
        {
            this.httpMethod = httpMethod;

            return this;
        }

        public HttpRequestMessageBuilder AddHeader(string name, string value)
        {
            headers.Add(new Header(name, value));

            return this;
        }

        public HttpRequestMessageBuilder SetHeader(string name, string value)
        {
            var header = headers.FirstOrDefault(x => x.Name == name);

            if (header != null)
            {
                headers.Remove(header);
            }

            AddHeader(name, value);

            return this;
        }

        public HttpRequestMessageBuilder SetStringContent(string content, Encoding encoding, string mediaType)
        {
            this.content = new StringContent(content, encoding, mediaType);

            return this;
        }

        public HttpRequestMessageBuilder SetHttpContent(HttpContent httpContent)
        {
            content = httpContent;

            return this;
        }

        public HttpRequestMessageBuilder AddUrlSegment(string name, string value)
        {
            segments.Add(new UrlSegment(name, value));

            return this;
        }

        public HttpRequestMessageBuilder AddQueryString(string key, string value)
        {
            parameters.Add(key, value);

            return this;
        }

        public HttpRequestMessage Build()
        {
            string resourceUrl = HttpRequestUtils.ReplaceSegments(url, segments);
            string queryString = HttpRequestUtils.BuildQueryString(resourceUrl, parameters);
            string resourceUrlWithQueryString = string.Format("{0}{1}", resourceUrl, queryString);

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(httpMethod, resourceUrlWithQueryString);

            foreach (var header in headers)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Name, header.Value);
            }

            if (content != null)
            {
                httpRequestMessage.Content = content;
            }

            return httpRequestMessage;
        }
    }
}