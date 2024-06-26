// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ElasticsearchCodeSearch.Git.Exceptions
{
    public class GitException : Exception
    {
        public readonly int ExitCode;
        public readonly string Errors;

        public GitException(int exitCode, string errors)
        {
            ExitCode = exitCode;
            Errors = errors;
        }
    }
}