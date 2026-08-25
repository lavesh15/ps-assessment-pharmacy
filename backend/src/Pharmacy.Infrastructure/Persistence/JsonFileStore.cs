using System.Text.Json;

namespace Pharmacy.Infrastructure.Persistence;

internal sealed class JsonFileStore<T>
{
    private readonly string _path;
    private readonly SemaphoreSlim _lock;

    public JsonFileStore(string path, SemaphoreSlim sharedLock)
    {
        _path = path;
        _lock = sharedLock;
    }

    public async Task<List<T>> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
        {
            return [];
        }

        await using var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, JsonFileSerializer.Options, cancellationToken);
        return items ?? [];
    }

    public async Task WriteAsync(List<T> items, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = _path + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, items, JsonFileSerializer.Options, cancellationToken);
        }

        File.Move(tempPath, _path, overwrite: true);
    }

    public async Task<TResult> MutateAsync<TResult>(Func<List<T>, TResult> mutator, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var items = await ReadAsync(cancellationToken);
            var result = mutator(items);
            await WriteAsync(items, cancellationToken);
            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<TResult> LockedAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return await action();
        }
        finally
        {
            _lock.Release();
        }
    }
}
