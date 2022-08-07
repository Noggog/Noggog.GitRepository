using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface IGitRepository : IDisposable
{
    IEnumerable<IBranch> Branches { get; }
    IEnumerable<ITag> Tags { get; }
    string CurrentSha { get; }
    IBranch CurrentBranch { get; }
    IBranch? MainBranch { get; }
    string WorkingDirectory { get; }
    string? MainRemoteUrl { get; }
    void Fetch();
    void ResetHard();
    void ResetHard(ICommit commit);
    ICommit? TryGetCommit(string sha, out bool validSha);
    IBranch TryCreateBranch(string branchName);
    bool TryGetBranch(string branchName, [MaybeNullWhen(false)] out IBranch branch);
    bool TryGetTagSha(string tagName, [MaybeNullWhen(false)] out string sha);
    void Checkout(IBranch branch);
    void Pull();
    void Stage(string path);
    void Commit(string message);
}

[ExcludeFromCodeCoverage]
public class GitRepository : IGitRepository
{
    private static Signature PlaceholderSignature = new("synthesis", "someemail@gmail.com", DateTimeOffset.Now);
    private readonly Repository _repository;

    public IEnumerable<IBranch> Branches => _repository.Branches.Select(x => new BranchWrapper(x));
    public IEnumerable<ITag> Tags => _repository.Tags.Select(x => new TagWrapper(x));
    public string CurrentSha => _repository.Head.Tip.Sha;
    public IBranch CurrentBranch => new BranchWrapper(_repository.Head);

    public IBranch? MainBranch
    {
        get
        {
            var ret = _repository.Branches.FirstOrDefault(b => b.IsCurrentRepositoryHead);
            if (ret == null) return null;
            return new BranchWrapper(ret);
        }
    }
        
    public string WorkingDirectory => _repository.Info.WorkingDirectory;
    public string? MainRemoteUrl => _repository.Network.Remotes.FirstOrDefault()?.Url;

    public GitRepository(Repository repository)
    {
        _repository = repository;
    }

    public void Fetch()
    {
        var fetchOptions = new FetchOptions();
        foreach (var remote in _repository.Network.Remotes)
        {
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            LibGit2Sharp.Commands.Fetch(_repository, remote.Name, refSpecs, fetchOptions, string.Empty);
        }
    }

    public void ResetHard()
    {
        _repository.Reset(ResetMode.Hard);
    }

    public void ResetHard(ICommit commit)
    {
        _repository.Reset(ResetMode.Hard, commit.GetUnderlying(), new CheckoutOptions());
    }

    public ICommit? TryGetCommit(string sha, out bool validSha)
    {
        validSha = ObjectId.TryParse(sha, out var objId);
        if (!validSha) return null;
        var ret = _repository.Lookup(objId, ObjectType.Commit) as Commit;
        if (ret == null) return null;
        return new CommitWrapper(ret);
    }

    public IBranch TryCreateBranch(string branchName)
    {
        return new BranchWrapper(
            _repository.Branches[branchName] ?? _repository.CreateBranch(branchName));
    }

    public bool TryGetBranch(string branchName, [MaybeNullWhen(false)] out IBranch branch)
    {
        var branchDirect = _repository.Branches[branchName];
        if (branchDirect != null)
        {
            branch = new BranchWrapper(branchDirect);
            return true;
        }

        branch = default;
        return false;
    }

    public bool TryGetTagSha(string tagName, [MaybeNullWhen(false)] out string sha)
    {
        sha = _repository.Tags[tagName]?.Target.Sha;
        return !sha.IsNullOrWhitespace();
    }

    public void Checkout(IBranch branch)
    {
        LibGit2Sharp.Commands.Checkout(_repository, branch.GetUnderlying());
    }
        
    public void Pull()
    {
        LibGit2Sharp.Commands.Pull(_repository, PlaceholderSignature, null);
    }

    public void Stage(string path)
    {
        LibGit2Sharp.Commands.Stage(_repository, path);
    }

    public void Commit(string message)
    {
        _repository.Commit(message, PlaceholderSignature, PlaceholderSignature);
    }

    public void Dispose()
    {
        _repository.Dispose();
    }
}