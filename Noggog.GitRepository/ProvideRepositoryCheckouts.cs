using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace Noggog.GitRepository;

public interface IProvideRepositoryCheckouts : IDisposable
{
    IRepositoryCheckout Get(DirectoryPath path);
}

public class ProvideRepositoryCheckouts : IProvideRepositoryCheckouts
{
    private readonly ILogger<ProvideRepositoryCheckouts> _logger;
    public IGitRepositoryFactory RepositoryFactory { get; }
        
    private readonly TaskCompletionSource _shutdown = new();
    private int _numInFlight;
    private readonly object _lock = new();

    public bool IsShutdownRequested { get; private set; }
    public bool IsShutdown { get; private set; }

    public ProvideRepositoryCheckouts(
        ILogger<ProvideRepositoryCheckouts> logger,
        IGitRepositoryFactory repositoryFactory)
    {
        _logger = logger;
        RepositoryFactory = repositoryFactory;
    }

    public IRepositoryCheckout Get(DirectoryPath path)
    {
        lock (_lock)
        {
            if (IsShutdown)
            {
                throw new InvalidOperationException("Tried to get a repository from a shut down manager");
            }

            _numInFlight++;
            return new RepositoryCheckout(
                new Lazy<IGitRepository>(() => RepositoryFactory.Get(path)),
                Disposable.Create(Cleanup));
        }
    }

    private void Cleanup()
    {
        lock (_lock)
        {
            _numInFlight--;
            if (IsShutdownRequested
                && _numInFlight == 0)
            {
                IsShutdown = true;
            }
        }

        if (IsShutdown)
        {
            _shutdown.TrySetResult();
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing repository jobs");
        lock (_lock)
        {
            IsShutdownRequested = true;
            if (_numInFlight == 0)
            {
                IsShutdown = true;
            }
            _logger.LogInformation("{NumInFlight} in flight repository jobs", _numInFlight == 0 ? "No" : _numInFlight);
        }

        if (IsShutdown)
        {
            _shutdown.TrySetResult();
        }

        _shutdown.Task.Wait();
        _logger.LogInformation("Finished disposing repository jobs");
    }
}