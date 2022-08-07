﻿using System.Diagnostics.CodeAnalysis;
using LibGit2Sharp;

namespace Noggog.GitRepository;

public interface ITag
{
    string FriendlyName { get; }
    string Sha { get; }
}

[ExcludeFromCodeCoverage]
public class TagWrapper : ITag
{
    private readonly Tag _tag;

    public string FriendlyName => _tag.FriendlyName;
    public string Sha => _tag.Target.Sha;

    public TagWrapper(Tag tag)
    {
        _tag = tag;
    }
}