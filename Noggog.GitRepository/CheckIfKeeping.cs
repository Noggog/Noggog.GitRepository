using System.IO.Abstractions;
using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface ICheckIfKeeping
{
    ErrorResponse ShouldKeep(
        DirectoryPath localDir,
        GetResponse<string> remoteUrl,
        Func<IGitRepository, ErrorResponse>? isDesirable = null);
}

public class CheckIfKeeping : ICheckIfKeeping
{
    private readonly IFileSystem _fileSystem;
    public IProvideRepositoryCheckouts RepoCheckouts { get; }

    public CheckIfKeeping(
        IFileSystem fileSystem,
        IProvideRepositoryCheckouts repoCheckouts)
    {
        _fileSystem = fileSystem;
        RepoCheckouts = repoCheckouts;
    }
        
    public ErrorResponse ShouldKeep(
        DirectoryPath localDir,
        GetResponse<string> remoteUrl,
        Func<IGitRepository, ErrorResponse>? isDesirable = null)
    {
        if (!localDir.CheckExists(_fileSystem))
        {
            return ErrorResponse.Fail("No local repository exists");
        }
        if (remoteUrl.Failed)
        {
            return ErrorResponse.Fail("No remote repository");
        }
        try
        {
            using var repoCheckout = RepoCheckouts.Get(localDir);

            var isDesirableResult = isDesirable?.Invoke(repoCheckout.Repository);
            if (isDesirableResult?.Failed ?? false) return isDesirableResult.Value;
                
            // If it's the same remote repo, don't delete
            if (repoCheckout.Repository.MainRemoteUrl?.Equals(remoteUrl.Value) ?? false)
            {
                return ErrorResponse.Succeed("Remote repository target matched local folder's repo");
            }
        }
        catch (RepositoryNotFoundException)
        {
            return ErrorResponse.Fail($"Repository corrupted");
        }

        return ErrorResponse.Fail("Remote address targeted a different repository");
    }
}