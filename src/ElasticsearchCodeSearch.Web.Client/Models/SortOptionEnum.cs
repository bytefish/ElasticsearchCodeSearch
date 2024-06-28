// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Web.Client.Models
{
    /// <summary>
    /// Sort Options for sorting search results.
    /// </summary>
    public enum SortOptionEnum
    {
        /// <summary>
        /// Sorts by Owner in ascending order.
        /// </summary>
        OwnerAscending = 1,

        /// <summary>
        /// Sorts by Owner in descending order.
        /// </summary>
        OwnerDescending = 2,

        /// <summary>
        /// Sorts by Repository in ascending order.
        /// </summary>
        RepositoryAscending = 3,

        /// <summary>
        /// Sorts by Respository in ascending order.
        /// </summary>
        RepositoryDescending = 4,

        /// <summary>
        /// Sorts by Latest Commit Date in ascending order.
        /// </summary>
        LatestCommitDateAscending = 5,

        /// <summary>
        /// Sorts by Latest Commit Date in descending order.
        /// </summary>
        LatestCommitDateDescending = 6,
    }
}
