using FluentAssertions;
using Noggog;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Noggog.GitRepository;
using Xunit;

namespace Synthesis.Bethesda.UnitTests.Execution.GitRepository;

public class CheckOrCloneRepoTests
{
    [Theory, DefaultAutoData]
    public void CallsCheckIfKeeping(
        ErrorResponse success,
        GetResponse<string> remote,
        DirectoryPath local,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ReturnsForAnyArgs(success);
        var ret = sut.Check(remote, local);
        sut.ShouldKeep.Received().ShouldKeep(local, remote);
        ret.Succeeded.Should().BeTrue();
        ret.Value.Remote.Should().Be(remote.Value);
        ret.Value.Local.Should().Be(local);
        sut.CloneRepo.DidNotReceiveWithAnyArgs().Clone(default!, default);
    }

    [Theory, DefaultAutoData]
    public void RemoteFailed(
        ErrorResponse failed,
        DirectoryPath local,
        GetResponse<string> failedRemote,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ReturnsForAnyArgs(failed);
        var ret = sut.Check(failedRemote, local);
        ret.Succeeded.Should().BeFalse();
        ret.Reason.Should().Be(failedRemote.Reason);
    }

    [Theory, DefaultAutoData]
    public void DeleteCalledIfRemoteFailed(
        ErrorResponse failed,
        DirectoryPath local,
        GetResponse<string> failedRemote,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ReturnsForAnyArgs(failed);
        sut.Check(failedRemote, local);
        sut.DeleteEntireDirectory.Received(1).DeleteEntireFolder(
            local, deleteFolderItself: true, disableReadOnly: true);
    }

    [Theory, DefaultAutoData]
    public void CheckIfKeepingThrows(
        DirectoryPath local,
        GetResponse<string> remote,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ThrowsForAnyArgs<NotImplementedException>();
        var ret = sut.Check(remote, local);
        ret.Succeeded.Should().BeFalse();
    }

    [Theory, DefaultAutoData]
    public void DeleteCalledIfNotKeeping(
        ErrorResponse failed,
        DirectoryPath local,
        GetResponse<string> remote,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ReturnsForAnyArgs(failed);
        sut.Check(remote, local);
        sut.DeleteEntireDirectory.Received(1).DeleteEntireFolder(
            local, disableReadOnly: true, deleteFolderItself: true);
    }

    [Theory, DefaultAutoData]
    public void CloneCalledIfNotKeeping(
        ErrorResponse failed,
        DirectoryPath local,
        GetResponse<string> remote,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ReturnsForAnyArgs(failed);
        sut.Check(remote, local);
        sut.CloneRepo.Received(1).Clone(remote.Value, local);
    }

    [Theory, DefaultAutoData]
    public void DeleteCalledBeforeReclone(
        ErrorResponse failed,
        DirectoryPath local,
        GetResponse<string> remote,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ReturnsForAnyArgs(failed);
        sut.Check(remote, local);
        Received.InOrder(() =>
        {
            sut.DeleteEntireDirectory.DeleteEntireFolder(
                Arg.Any<DirectoryPath>(),
                Arg.Any<bool>(),
                Arg.Any<bool>());
            sut.CloneRepo.Clone(
                Arg.Any<string>(), 
                Arg.Any<DirectoryPath>());
        });
    }

    [Theory, DefaultAutoData]
    public void ReturnsRemoteAndLocal(
        ErrorResponse failed,
        DirectoryPath local,
        GetResponse<string> remote,
        CheckOrCloneRepo sut)
    {
        sut.ShouldKeep.ShouldKeep(default, default).ReturnsForAnyArgs(failed);
        sut.Check(
            remote,
            local);
        sut.CloneRepo.Received(1).Clone(remote.Value, local);
    }
}