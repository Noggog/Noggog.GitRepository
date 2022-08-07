using Xunit;
using FluentAssertions;
using LibGit2Sharp;
using Noggog;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Noggog.GitRepository;

namespace Synthesis.Bethesda.UnitTests.Execution.GitRepository;

public class CheckIfKeepingTests
{
    [Theory, DefaultAutoData]
    public void LocalDirDoesNotExistReturnsFalse(
        DirectoryPath missingDir,
        GetResponse<string> remoteUrl,
        CheckIfKeeping sut)
    {
        sut.ShouldKeep(missingDir, remoteUrl)
            .Succeeded.Should().BeFalse();
    }
        
    [Theory, DefaultAutoData]
    public void RemoteUrlFailedReturnsFalse(
        DirectoryPath existingDir,
        GetResponse<string> failedRemote,
        CheckIfKeeping sut)
    {
        sut.ShouldKeep(existingDir, failedRemote)
            .Succeeded.Should().BeFalse();
    }
        
    [Theory, DefaultAutoData]
    public void PassesLocalDirToCheckout(
        DirectoryPath existingDir,
        GetResponse<string> remote,
        CheckIfKeeping sut)
    {
        sut.ShouldKeep(existingDir, remote);
        sut.RepoCheckouts.Received(1).Get(existingDir);
    }
        
    [Theory, DefaultAutoData]
    public void UndesirableReturnsFalse(
        DirectoryPath existingDir,
        GetResponse<string> remote,
        CheckIfKeeping sut)
    {
        sut.ShouldKeep(existingDir, remote, x => ErrorResponse.Failure)
            .Succeeded.Should().BeFalse();
    }
        
    [Theory, DefaultAutoData]
    public void DesirableReturnsTrue(
        DirectoryPath existingDir,
        GetResponse<string> remote,
        IRepositoryCheckout checkout,
        CheckIfKeeping sut)
    {
        sut.RepoCheckouts.Get(default).ReturnsForAnyArgs(checkout);
        checkout.Repository.MainRemoteUrl.Returns(remote.Value);

        sut.ShouldKeep(existingDir, remote, isDesirable: (x) => ErrorResponse.Success)
            .Succeeded.Should().BeTrue();
    }
        
    [Theory, DefaultAutoData]
    public void NoDesirablityReturnsTrue(
        DirectoryPath existingDir,
        GetResponse<string> remote,
        IRepositoryCheckout checkout,
        CheckIfKeeping sut)
    {
        sut.RepoCheckouts.Get(default).ReturnsForAnyArgs(checkout);
        checkout.Repository.MainRemoteUrl.Returns(remote.Value);

        sut.ShouldKeep(existingDir, remote, isDesirable: null)
            .Succeeded.Should().BeTrue();
    }
        
    [Theory, DefaultAutoData]
    public void SameRepositoryAddressReturnsTrue(
        DirectoryPath existingDir,
        GetResponse<string> remote,
        IRepositoryCheckout checkout,
        CheckIfKeeping sut)
    {
        sut.RepoCheckouts.Get(default).ReturnsForAnyArgs(checkout);
        checkout.Repository.MainRemoteUrl.Returns(remote.Value);
            
        sut.ShouldKeep(existingDir, remote)
            .Succeeded.Should().BeTrue();
    }
        
    [Theory, DefaultAutoData]
    public void DifferentRepositoryAddressReturnsTrue(
        DirectoryPath existingDir,
        GetResponse<string> remote,
        IRepositoryCheckout checkout,
        string otherAddress,
        CheckIfKeeping sut)
    {
        checkout.Repository.MainRemoteUrl.Returns(otherAddress);
            
        sut.ShouldKeep(existingDir, remote)
            .Succeeded.Should().BeFalse();
    }
        
    [Theory, DefaultAutoData]
    public void RepositoryNotFoundExceptionReturnsFalse(
        DirectoryPath existingDir,
        GetResponse<string> remote,
        CheckIfKeeping sut)
    {
        sut.RepoCheckouts.Get(default).ThrowsForAnyArgs<RepositoryNotFoundException>();
            
        sut.ShouldKeep(existingDir, remote)
            .Succeeded.Should().BeFalse();
    }
}