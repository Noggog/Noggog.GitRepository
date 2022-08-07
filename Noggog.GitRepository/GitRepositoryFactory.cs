using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface IGitRepositoryFactory
{
    IGitRepository Get(DirectoryPath path);
}

public class GitRepositoryFactory : IGitRepositoryFactory
{
    public IGitRepository Get(DirectoryPath path)
    {
        return new GitRepository(new Repository(path));
    }
}