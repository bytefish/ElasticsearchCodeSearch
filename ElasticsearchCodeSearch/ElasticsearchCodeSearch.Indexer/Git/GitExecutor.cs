// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ElasticsearchCodeSearch.Indexer.Git.Exceptions;
using ElasticsearchCodeSearch.Shared.Logging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace ElasticsearchCodeSearch.Indexer.Git
{
    public class GitExecutor
    {
        private readonly ILogger<GitExecutor> _logger;

        public GitExecutor(ILogger<GitExecutor> logger) 
        {
            _logger = logger;
        }

        public async Task Clone(string repository_url, string repository_directory, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            await RunAsync($"clone {repository_url} {repository_directory}", string.Empty, cancellationToken);
        }

        public async Task<string> SHA1(string repository_directory, string path, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($"ls-files -s \"{path}\"", repository_directory, cancellationToken);

            return result.Split(" ").Skip(1).First();
        }

        public async Task<string> CommitHash(string repository_directory, string path, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($"log --pretty=format:\"%H\" -n 1 -- \"{path}\"", repository_directory, cancellationToken);

            return result;
        }

        public async Task<DateTime> LatestCommitDate(string repository_directory, string path, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($" log -1  --date=iso-strict --format=\"%ad\" -- \"{path}\"", repository_directory, cancellationToken);

            if(DateTime.TryParse(result, out var date))
            {
                return date;
            }

            return default;
        }

        public async Task<string[]> ListFiles(string repository_directory, CancellationToken cancellationToken)
        {
            _logger.TraceMethodEntry();

            var result = await RunAsync($"ls-files", repository_directory, cancellationToken);

            var files = result
                .Split("\r\n")
                .ToArray();

            return files;
        }

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