using Microsoft.AspNetCore.Http;

namespace InventoryManagementSystemUnitTest.HelperMethods
{
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _storage = new();

        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _storage.Keys;

        public void Clear() => _storage.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _storage.Remove(key);

        public void Set(string key, byte[] value) => _storage[key] = value;

        public bool TryGetValue(string key, out byte[] value) => _storage.TryGetValue(key, out value);
    }
}
