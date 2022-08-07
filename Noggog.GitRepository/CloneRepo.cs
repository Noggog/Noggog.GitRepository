﻿using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface ICloneRepo
{
    DirectoryPath Clone(string repoPath, DirectoryPath localDir);
}

[ExcludeFromCodeCoverage]
public class CloneRepo : ICloneRepo
{
    public DirectoryPath Clone(string repoPath, DirectoryPath localDir)
    {
        return Repository.Clone(repoPath, localDir);
    }
}