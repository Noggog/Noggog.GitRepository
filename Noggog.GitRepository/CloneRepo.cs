using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface ICloneRepo
{
    void Clone(string repoPath, DirectoryPath localDir);
}

[ExcludeFromCodeCoverage]
public class CloneRepo : ICloneRepo
{
    public void Clone(string repoPath, DirectoryPath localDir)
    {
        Repository.Clone(repoPath, localDir);
    }
}