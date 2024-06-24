using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticsearchCodeSearch.Shared.Dto
{
    public class CodeIndexQueueDto
    {
        /// <summary>
        /// Gets or sets the current repositories to index.
        /// </summary>
        public required List<string> Repositories { get; set; } = [];

        /// <summary>
        /// Gets or sets the current organizations to index.
        /// </summary>
        public required List<string> Organizations { get; set; } = [];

        /// <summary>
        /// Gets or sets the current urls to index.
        /// </summary>
        public required List<string> Urls { get; set; } = [];
    }
}
