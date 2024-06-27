// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Git.Exceptions;
using ElasticsearchCodeSearch.Shared.Logging;
using System.Diagnostics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ElasticsearchCodeSearch.Git
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

            try
            {
                _logger.LogDebug("Start Cloning Git Url '{CloneUrl}' to Directory '{RepositoryDirectory}'", repositoryUrl, repositoryDirectory);

                await RunGitAsync($"clone {repositoryUrl} {repositoryDirectory}", string.Empty, cancellationToken).ConfigureAwait(false);

                _logger.LogDebug("Finished Cloning Git Url '{CloneUrl}' to Directory '{RepositoryDirectory}'", repositoryUrl, repositoryDirectory);

            }
            catch (GitException e)
            {
                _logger.LogError("The Git CLI failed (ExitCode = {GitErrorCode}, Errors = {ErrorMessage})", e.ExitCode, e.Errors);

                throw;
            }
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

            var result = await RunGitAsync($"ls-files -s \"{path}\"", repositoryDirectory, cancellationToken).ConfigureAwait(false);

            // The Output looks like this <Irrelevant> <SHA1 Hash> <Irrelevant> <Irrelevant>
            var components = result.Split(" ");

            if (components.Length < 2)
            {
                if (_logger.IsWarningEnabled())
                {
                    var absoluteFilename = Path.Combine(repositoryDirectory, path);

                    _logger.LogWarning("Could not determine the SHA1 Hash for '{File}'. Raw Git Output was: '{GitOutput}'", absoluteFilename, result);
                }

                return string.Empty;
            }

            var hashValue = components.Skip(1).First();

            if (_logger.IsTraceEnabled())
            {
                _logger.LogTrace("Determined Blob / File Hash (Repository = '{RepositoryDirectory}', Path '{RelativeFilename}', Sha1 = '{FileHash}')",
                    repositoryDirectory, path, hashValue);
            }

            return hashValue;
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

            var result = await RunGitAsync($"log --pretty=format:\"%H\" -n 1 -- \"{path}\"", repositoryDirectory, cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(result))
            {
                if (_logger.IsWarningEnabled())
                {
                    var absoluteFilename = Path.Combine(repositoryDirectory, path);

                    _logger.LogWarning("Could not determine the Commit Hash for '{File}'. Raw Git Output was: '{GitOutput}'", absoluteFilename, result);
                }

                return string.Empty;
            }

            if (_logger.IsTraceEnabled())
            {
                _logger.LogTrace("Determined Commit Hash (Repository = '{RepositoryDirectory}', Path '{RelativeFilename}', Sha1 = '{CommitHash}')",
                    repositoryDirectory, path, result);
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

            var result = await RunGitAsync($"log -1  --date=iso-strict --format=\"%ad\" -- \"{path}\"", repositoryDirectory, cancellationToken).ConfigureAwait(false);

            if (!DateTime.TryParse(result, out var date))
            {
                if (_logger.IsWarningEnabled())
                {
                    var absoluteFilename = Path.Combine(repositoryDirectory, path);

                    _logger.LogWarning("Could not convert the Latest Commit Date to a DateTime for '{File}'. Raw Git Output was: '{GitOutput}'", absoluteFilename, result);
                }

                return default;
            }

            if(_logger.IsTraceEnabled())
            {
                _logger.LogTrace("Determined Commit Date (Repository = '{RepositoryDirectory}', Path '{RelativeFilename}', Date = '{LatestCommitDate}')",
                    repositoryDirectory, path, date);
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

            if(_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Listing Files in Directory '{RepositoryDirectory}'", repositoryDirectory);
            }

            var result = await RunGitAsync($"ls-files", repositoryDirectory, cancellationToken).ConfigureAwait(false);

            var files = result
                .Split(Environment.NewLine)
                .ToArray();

            if(_logger.IsTraceEnabled())
            {
                _logger.LogTrace("Git listed the following files in directory '{RepositoryDirectory}': {FilesInGitRepository}", repositoryDirectory, files);
            }

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
        public async Task<string> RunGitAsync(string arguments, string workingDirectory, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            if (_logger.IsDebugEnabled())
            {
                _logger.LogDebug("Executing Git (Arguments = '{GitCliArguments}', WorkingDirectory = '{GitCliWorkingDirectory}')", arguments, workingDirectory);
            }

            var result = await RunProcessAsync("git", arguments, workingDirectory, cancellationToken).ConfigureAwait(false);

            if (result.ExitCode != 0)
            {
                throw new GitException(result.ExitCode, result.Errors);
            }

            // In Trace we want to also print the output. Not sure, if this isn't too large. It's the file content
            // after all. It's commented out for now, because I can probably make sense of it manually.
            if (_logger.IsTraceEnabled())
            {
                _logger.LogTrace("Git Executable finished (Arguments = {GitCliArguments}, WorkingDirectory = {GitCliWorkingDirectory}, ExitCode = {GitCliExitCode}, Errors = {GitCliErrors}, Output = {GitCliOutput})",
                    arguments, workingDirectory, result.ExitCode, result.Errors, result.Output);
            }

            // In Debug, we omit the output. There might be a better way to enrich the logs depending on level.
            if (!_logger.IsTraceEnabled() && _logger.IsDebugEnabled())
            {
                _logger.LogDebug("Git Executable finished (Arguments = {GitCliArguments}, WorkingDirectory = {GitCliWorkingDirectory}, ExitCode = {GitCliExitCode}, Errors = {GitCliErrors})", arguments, workingDirectory, result.ExitCode, result.Errors);
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

                await process
                    .WaitForExitAsync(cancellationToken)
                    .ConfigureAwait(false);

                var exitCode = process.ExitCode;
                var output = outputBuilder.ToString().Trim();
                var errors = errorsBuilder.ToString().Trim();

                return (exitCode, output, errors);
            }
        }
    }
}