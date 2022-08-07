using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface ICheckLocalRepoIsValid
{
    bool IsValidRepository([NotNullWhen(true)]DirectoryPath? dir);
}

public class CheckLocalRepoIsValid : ICheckLocalRepoIsValid
{
    public bool IsValidRepository([NotNullWhen(true)]DirectoryPath? dir)
    {
        if (dir == null) return false;
        return Repository.IsValid(dir);
    }
}