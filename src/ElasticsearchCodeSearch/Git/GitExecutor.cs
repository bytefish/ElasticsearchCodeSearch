// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.Git.Exceptions;
using ElasticsearchCodeSearch.Shared.Logging;
using System.Diagnostics;
using System.Text;

namespace ElasticsearchCodeSearch.Indexer.Git
{
    /// <summary>
    /// Exposes various GIT commands useful for indexing files.
    /// </summary>
    public class GitExecutor
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<GitExecutor> _logger;

        /// <summary>
        /// Creates a new GitExecutor.
        /// </summary>
        /// <param name="logger">Logger</param>
        public GitExecutor(ILogger<GitExecutor> logger) 
        {
            _logger = logger;
        }

        /// <summary>
        /// Clones a Repository 
        /// </summary>
        /// <param name="repositoryUrl"></param>
        /// <param name="repositoryDirectory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Clone(string repositoryUrl, string repositoryDirectory, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await RunAsync($"clone {repositoryUrl} {repositoryDirectory}", string.Empty, cancellationToken);
        }

        /// <summary>
        /// Gets the SHA1 Hash for the file.
        /// </summary>
        /// <param name="repositoryDirectory">The Working Directory</param>
        /// <param name="path">Relative path to the file</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>SHA1 Hash for the file</returns>
        public async Task<string> SHA1(string repositoryDirectory, string path, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($"ls-files -s \"{path}\"", repositoryDirectory, cancellationToken);

            // The Output looks like this <Irrelevant> <SHA1 Hash> <Irrelevant> <Irrelevant>
            var components = result.Split(" ");

            if (components.Length < 2)
            {
                if(_logger.IsWarningEnabled())
                {
                    var absoluteFilename = Path.Combine(repositoryDirectory, path);

                    _logger.LogWarning("Could not determine the SHA1 Hash for '{File}'. Raw Git Output was: '{GitOutput}'", absoluteFilename, result);
                }

                return string.Empty;
            }

            return components.Skip(1).First();
        }

        /// <summary>
        /// Gets the Commit Hash for a file, which is the following git command:
        /// 
        ///     log --pretty=format:"%H" -n 1 -- "{path}"
        ///     
        /// </summary>
        /// <param name="repositoryDirectory">The Working Directory</param>
        /// <param name="path">Relative path to the file</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>SHA1 Commit Hash for the given file</returns>
        public async Task<string> CommitHash(string repositoryDirectory, string path, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($"log --pretty=format:\"%H\" -n 1 -- \"{path}\"", repositoryDirectory, cancellationToken);

            if (string.IsNullOrWhiteSpace(result))
            {
                if (_logger.IsWarningEnabled())
                {
                    var absoluteFilename = Path.Combine(repositoryDirectory, path);

                    _logger.LogWarning("Could not determine the Commit Hash for '{File}'. Raw Git Output was: '{GitOutput}'", absoluteFilename, result);
                }

                return string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Gets the latest commit date for a file, which is the following git command:
        ///     
        ///     log -1  --date=iso-strict --format=\"%ad\" -- "{path}"
        ///     
        /// </summary>
        /// <param name="repositoryDirectory">The Working Directory</param>
        /// <param name="path">Relative path to the file</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Latest Commit Date for a file</returns>
        public async Task<DateTime> LatestCommitDate(string repositoryDirectory, string path, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($"log -1  --date=iso-strict --format=\"%ad\" -- \"{path}\"", repositoryDirectory, cancellationToken);

            if(!DateTime.TryParse(result, out var date))
            {
                if (_logger.IsWarningEnabled())
                {
                    var absoluteFilename = Path.Combine(repositoryDirectory, path);

                    _logger.LogWarning("Could not convert the Latest Commit Date to a DateTime for '{File}'. Raw Git Output was: '{GitOutput}'", absoluteFilename, result);
                }

                return default;
            }

            return date;
        }

        /// <summary>
        /// Lists all files in a given Git Repository, which is the git command:
        ///     
        ///     ls-files 
        ///     
        /// </summary>
        /// <param name="repositoryDirectory">Repository</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>List of Files for the repository</returns>
        public async Task<string[]> ListFiles(string repositoryDirectory, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($"ls-files", repositoryDirectory, cancellationToken);

            var files = result
                .Split(Environment.NewLine)
                .ToArray();

            return files;
        }

        /// <summary>
        /// Runs a GIT command in a given working directory.
        /// </summary>
        /// <param name="arguments">Command Line Arguments for the git executable</param>
        /// <param name="workingDirectory">Working Directory to execute in</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>Output of the git command</returns>
        /// <exception cref="GitException">Thrown, if the git process has an exit code other that 0</exception>
        public async Task<string> RunAsync(string arguments, string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunProcessAsync("git", arguments, workingDirectory, cancellationToken);

            if(result.ExitCode != 0)
            {
                throw new GitException(result.ExitCode, result.Errors);
            }

            return result.Output;
        }

        /// <summary>
        /// Executes a Process for a given application, with arguments and a working directory.
        /// </summary>
        /// <param name="application">Application to execute a Process for</param>
        /// <param name="arguments">Arguments for the Process</param>
        /// <param name="workingDirectory">Working Directory</param>
        /// <param name="cancellationToken">Cancellation Token</param>
        /// <returns>The exit code, output and errors, if any</returns>
        private async Task<(int ExitCode, string Output, string Errors)> RunProcessAsync(string application, string arguments, string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = application,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                };

                var outputBuilder = new StringBuilder();
                var errorsBuilder = new StringBuilder();

                process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
                process.ErrorDataReceived += (_, args) => errorsBuilder.AppendLine(args.Data);

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(cancellationToken);

                var exitCode = process.ExitCode;
                var output = outputBuilder.ToString().Trim();
                var errors = errorsBuilder.ToString().Trim();

                return (exitCode, output, errors);
            }
        }
    }
}