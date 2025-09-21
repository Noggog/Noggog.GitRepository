using LibGit2Sharp;
using Noggog;
using Noggog.GitRepository;
using Shouldly;
using System.IO;
using Noggog.IO;
using Xunit;

namespace Synthesis.Bethesda.UnitTests.Execution.GitRepository;

public class HasUncommittedChangesTests
{
    private class TestRepository : IDisposable
    {
        public DirectoryPath TempPath { get; }
        public IGitRepository GitRepository { get; }
        private TempFolder _tempFolder;

        public TestRepository()
        {
            _tempFolder = TempFolder.FactoryByAddedPath(nameof(HasUncommittedChangesTests));
            TempPath = _tempFolder.Dir;
            Repository.Init(TempPath);
            var libGit2Repository = new Repository(TempPath);
            GitRepository = new Noggog.GitRepository.GitRepository(libGit2Repository);
        }

        public void Dispose()
        {
            GitRepository.Dispose();
            _tempFolder.Dispose();
        }
    }

    private TestRepository CreateTestRepository() => new TestRepository();

    private void CreateInitialCommit(IGitRepository gitRepository, DirectoryPath tempPath)
    {
        var initialFilePath = Path.Combine(tempPath, "initial.txt");
        File.WriteAllText(initialFilePath, "Initial content");
        gitRepository.Stage("initial.txt");
        gitRepository.Commit("Initial commit");
    }

    [Fact]
    public void HasUncommittedChanges_EmptyRepository_ReturnsFalse()
    {
        using var testRepo = CreateTestRepository();

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeFalse();
    }

    [Fact]
    public void HasUncommittedChanges_WithUntrackedFile_ReturnsTrue()
    {
        using var testRepo = CreateTestRepository();

        var testFilePath = Path.Combine(testRepo.TempPath, "test.txt");
        File.WriteAllText(testFilePath, "Test content");

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeTrue();
    }

    [Fact]
    public void HasUncommittedChanges_WithStagedFile_ReturnsTrue()
    {
        using var testRepo = CreateTestRepository();

        var testFilePath = Path.Combine(testRepo.TempPath, "test.txt");
        File.WriteAllText(testFilePath, "Test content");

        testRepo.GitRepository.Stage("test.txt");

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeTrue();
    }

    [Fact]
    public void HasUncommittedChanges_AfterCommit_ReturnsFalse()
    {
        using var testRepo = CreateTestRepository();

        CreateInitialCommit(testRepo.GitRepository, testRepo.TempPath);

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeFalse();
    }

    [Fact]
    public void HasUncommittedChanges_WithModifiedTrackedFile_ReturnsTrue()
    {
        using var testRepo = CreateTestRepository();

        CreateInitialCommit(testRepo.GitRepository, testRepo.TempPath);

        var testFilePath = Path.Combine(testRepo.TempPath, "test.txt");
        File.WriteAllText(testFilePath, "Initial content");

        testRepo.GitRepository.Stage("test.txt");
        testRepo.GitRepository.Commit("Add test file");

        File.WriteAllText(testFilePath, "Modified content");

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeTrue();
    }

    [Fact]
    public void HasUncommittedChanges_WithDeletedTrackedFile_ReturnsTrue()
    {
        using var testRepo = CreateTestRepository();

        CreateInitialCommit(testRepo.GitRepository, testRepo.TempPath);

        var testFilePath = Path.Combine(testRepo.TempPath, "test.txt");
        File.WriteAllText(testFilePath, "Initial content");

        testRepo.GitRepository.Stage("test.txt");
        testRepo.GitRepository.Commit("Add test file");

        File.Delete(testFilePath);

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeTrue();
    }

    [Fact]
    public void HasUncommittedChanges_WithStagedModification_ReturnsTrue()
    {
        using var testRepo = CreateTestRepository();

        CreateInitialCommit(testRepo.GitRepository, testRepo.TempPath);

        var testFilePath = Path.Combine(testRepo.TempPath, "test.txt");
        File.WriteAllText(testFilePath, "Initial content");

        testRepo.GitRepository.Stage("test.txt");
        testRepo.GitRepository.Commit("Add test file");

        File.WriteAllText(testFilePath, "Modified content");
        testRepo.GitRepository.Stage("test.txt");

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeTrue();
    }

    [Fact]
    public void HasUncommittedChanges_MultipleFiles_ReturnsTrue()
    {
        using var testRepo = CreateTestRepository();

        var testFile1Path = Path.Combine(testRepo.TempPath, "test1.txt");
        var testFile2Path = Path.Combine(testRepo.TempPath, "test2.txt");

        File.WriteAllText(testFile1Path, "Content 1");
        File.WriteAllText(testFile2Path, "Content 2");

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeTrue();
    }

    [Fact]
    public void HasUncommittedChanges_AfterResetHard_ReturnsFalse()
    {
        using var testRepo = CreateTestRepository();

        CreateInitialCommit(testRepo.GitRepository, testRepo.TempPath);

        var testFilePath = Path.Combine(testRepo.TempPath, "test.txt");
        File.WriteAllText(testFilePath, "Initial content");

        testRepo.GitRepository.Stage("test.txt");
        testRepo.GitRepository.Commit("Add test file");

        File.WriteAllText(testFilePath, "Modified content");
        testRepo.GitRepository.HasUncommittedChanges.ShouldBeTrue();

        testRepo.GitRepository.ResetHard();

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeFalse();
    }

    [Fact]
    public void HasUncommittedChanges_WithIgnoredFile_ReturnsFalse()
    {
        using var testRepo = CreateTestRepository();

        CreateInitialCommit(testRepo.GitRepository, testRepo.TempPath);

        var gitignorePath = Path.Combine(testRepo.TempPath, ".gitignore");
        File.WriteAllText(gitignorePath, "ignored.txt\n");

        testRepo.GitRepository.Stage(".gitignore");
        testRepo.GitRepository.Commit("Add gitignore");

        var ignoredFilePath = Path.Combine(testRepo.TempPath, "ignored.txt");
        File.WriteAllText(ignoredFilePath, "This should be ignored");

        testRepo.GitRepository.HasUncommittedChanges.ShouldBeFalse();
    }
}