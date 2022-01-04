using CliFx.Exceptions;
using CliWrap;
using CliWrap.Buffered;
using Husky.Helpers;
using Husky.Stdout;

namespace Husky;

public class Git
{
   private readonly AsyncLazy<string> _gitDirRelativePath;
   private readonly AsyncLazy<string> _gitPath;
   private readonly AsyncLazy<string> _currentBranch;
   private readonly AsyncLazy<string> _huskyPath;
   private readonly AsyncLazy<string[]> _stagedFiles;
   private readonly AsyncLazy<string[]> _lastCommitFiles;
   private readonly AsyncLazy<string[]> _GitFiles;

   public async Task<string[]> GetStagedFilesAsync() => await _stagedFiles;
   public async Task<string[]> GitFilesAsync() => await _GitFiles;
   public async Task<string[]> GetLastCommitFilesAsync() => await _lastCommitFiles;
   public async Task<string> GetGitPathAsync() => await _gitPath;
   public async Task<string> GetGitDirRelativePathAsync() => await _gitDirRelativePath;
   public async Task<string> GetCurrentBranchAsync() => await _currentBranch;
   public async Task<string> GetHuskyPathAsync() => await _huskyPath;

   public Git()
   {
      _gitPath = new AsyncLazy<string>(GetGitPath);
      _huskyPath = new AsyncLazy<string>(GetHuskyPath);
      _stagedFiles = new AsyncLazy<string[]>(GetStagedFiles);
      _GitFiles = new AsyncLazy<string[]>(GetGitFiles);
      _lastCommitFiles = new AsyncLazy<string[]>(GetLastCommitFiles);
      _currentBranch = new AsyncLazy<string>(GetCurrentBranch);
      _gitDirRelativePath = new AsyncLazy<string>(GetGitDirRelativePath);
   }

   private static async Task<string> GetGitDirRelativePath()
   {
      try
      {
         var result = await ExecBufferedAsync("rev-parse --path-format=relative --git-dir");
         if (result.ExitCode != 0)
            throw new Exception($"Exit code: {result.ExitCode}"); // break execution

         return result.StandardOutput.Trim();
      }
      catch (Exception e)
      {
         e.Message.LogVerbose(ConsoleColor.DarkRed);
         throw new CommandException("Could not find git directory", innerException: e);
      }
   }

   private static async Task<string> GetCurrentBranch()
   {
      try
      {
         var result = await ExecBufferedAsync("branch --show-current");
         if (result.ExitCode != 0)
            throw new Exception($"Exit code: {result.ExitCode}"); // break execution

         return result.StandardOutput.Trim();
      }
      catch (Exception e)
      {
         e.Message.LogVerbose(ConsoleColor.DarkRed);
         throw new CommandException("Could not find git path", innerException: e);
      }
   }

   public static Task<CommandResult> ExecAsync(string args)
   {
      return Utility.ExecDirectAsync("git", args);
   }

   public static Task<BufferedCommandResult> ExecBufferedAsync(string args)
   {
      return Utility.ExecBufferedAsync("git", args);
   }

   private static async Task<string> GetHuskyPath()
   {
      try
      {
         var result = await ExecBufferedAsync("config --get core.hooksPath");
         if (result.ExitCode != 0)
            throw new Exception($"Exit code: {result.ExitCode}"); // break execution

         return result.StandardOutput.Trim();
      }
      catch (Exception e)
      {
         e.Message.LogVerbose(ConsoleColor.DarkRed);
         throw new CommandException("Could not find Husky path", innerException: e);
      }
   }

   private static async Task<string> GetGitPath()
   {
      try
      {
         var result = await ExecBufferedAsync("rev-parse --show-toplevel");
         if (result.ExitCode != 0)
            throw new Exception($"Exit code: {result.ExitCode}"); // break execution

         return result.StandardOutput.Trim();
      }
      catch (Exception e)
      {
         e.Message.LogVerbose(ConsoleColor.DarkRed);
         throw new CommandException("Could not find git path", innerException: e);
      }
   }

   private static async Task<string[]> GetLastCommitFiles()
   {
      try
      {
         var result = await ExecBufferedAsync("diff --diff-filter=d --name-only HEAD^");
         if (result.ExitCode != 0)
            throw new Exception($"Exit code: {result.ExitCode}"); // break execution

         return result.StandardOutput.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      }
      catch (Exception e)
      {
         e.Message.LogVerbose(ConsoleColor.DarkRed);
         throw new CommandException("Could not find the last commit files", innerException: e);
      }
   }

   private static async Task<string[]> GetStagedFiles()
   {
      try
      {
         var result = await ExecBufferedAsync("diff --diff-filter=d --name-only --staged");
         if (result.ExitCode != 0)
            throw new Exception($"Exit code: {result.ExitCode}"); // break execution

         return result.StandardOutput.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      }
      catch (Exception e)
      {
         e.Message.LogVerbose(ConsoleColor.DarkRed);
         throw new CommandException("Could not find the staged files", innerException: e);
      }
   }

   private static async Task<string[]> GetGitFiles()
   {
      try
      {
         var result = await ExecBufferedAsync("ls-files");
         if (result.ExitCode != 0)
            throw new Exception($"Exit code: {result.ExitCode}"); // break execution

         return result.StandardOutput.Trim().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      }
      catch (Exception e)
      {
         e.Message.LogVerbose(ConsoleColor.DarkRed);
         throw new CommandException("Could not find the committed files", innerException: e);
      }
   }
}
