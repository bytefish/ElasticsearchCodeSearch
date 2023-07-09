// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Client.Http.Builder
{
    public class UrlSegment
    {
        public readonly string name;
        public readonly string value;

        public UrlSegment(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}
