using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface IInitRepository
{
    bool Init(DirectoryPath folder);
}

public class InitRepository : IInitRepository
{
    public bool Init(DirectoryPath folder)
    {
        if (!Repository.IsValid(folder))
        {
            Repository.Init(folder);
            return true;
        }

        return false;
    }
}