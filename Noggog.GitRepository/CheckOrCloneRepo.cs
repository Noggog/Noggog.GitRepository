using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Noggog.IO;

namespace Noggog.GitRepository;

[ExcludeFromCodeCoverage]
public record RepoPathPair(string Remote, DirectoryPath Local);
    
public interface ICheckOrCloneRepo
{
    GetResponse<RepoPathPair> Check(
        GetResponse<string> remote,
        DirectoryPath localDir,
        Func<IGitRepository, ErrorResponse>? isDesirable = null,
        CancellationToken cancel = default);
}

public class CheckOrCloneRepo : ICheckOrCloneRepo
{
    private readonly ILogger<CheckOrCloneRepo> _logger;
    public ICloneRepo CloneRepo { get; }
    public IDeleteEntireDirectory DeleteEntireDirectory { get; }
    public ICheckIfKeeping ShouldKeep { get; }

    public CheckOrCloneRepo(
        ILogger<CheckOrCloneRepo> logger,
        ICloneRepo cloneRepo,
        IDeleteEntireDirectory deleteEntireDirectory,
        ICheckIfKeeping shouldKeep)
    {
        _logger = logger;
        CloneRepo = cloneRepo;
        DeleteEntireDirectory = deleteEntireDirectory;
        ShouldKeep = shouldKeep;
    }
        
    public GetResponse<RepoPathPair> Check(
        GetResponse<string> remote,
        DirectoryPath localDir,
        Func<IGitRepository, ErrorResponse>? isDesirable = null,
        CancellationToken cancel = default)
    {
        try
        {
            cancel.ThrowIfCancellationRequested();
            var shouldKeep = ShouldKeep.ShouldKeep(localDir: localDir, remoteUrl: remote, isDesirable: isDesirable);
            if (shouldKeep.Succeeded)
            {
                return GetResponse<RepoPathPair>.Succeed(new(remote.Value, localDir), remote.Reason);
            }

            _logger.LogInformation("Not keeping local repository at {LocalDir}: {Reason}", localDir, shouldKeep.Reason);
            cancel.ThrowIfCancellationRequested();
                
            DeleteEntireDirectory.DeleteEntireFolder(localDir);
            cancel.ThrowIfCancellationRequested();
                
            if (remote.Failed) return GetResponse<RepoPathPair>.Fail(new(remote.Value, string.Empty), remote.Reason);
            _logger.LogInformation("Cloning remote {RemotePath}", remote.Value);
            var clonePath = CloneRepo.Clone(remote.Value, localDir);
            return GetResponse<RepoPathPair>.Succeed(new(remote.Value, clonePath), remote.Reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure while checking/cloning repository");
            return GetResponse<RepoPathPair>.Fail(new(remote.Value, string.Empty), ex);
        }
    }
}