using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface ICommit
{
    string Sha { get; }
    string CommitMessage { get; }
    DateTime CommitDate { get; }
    IEnumerable<ICommit> Parents { get; }
    internal Commit GetUnderlying();
}

public class CommitWrapper : ICommit
{
    private readonly Commit _commit;
    public string Sha => _commit.Sha;
    public string CommitMessage => _commit.Message;
    public DateTime CommitDate => _commit.Author.When.LocalDateTime;
    public IEnumerable<ICommit> Parents => _commit.Parents.Select(x => new CommitWrapper(x));

    public CommitWrapper(Commit commit)
    {
        _commit = commit;
    }

    public Commit GetUnderlying() => _commit;
}