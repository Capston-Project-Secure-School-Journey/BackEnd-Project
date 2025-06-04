namespace Api.Hubs;

public class ConnectionMapping<T> where T : class
{
    private readonly Dictionary<T, HashSet<string>> _connections = new();

    public int Count
    {
        get
        {
            lock (_connections)
            {
                return _connections.Count;
            }
        }
    }

    public void Add(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                connections = [];
                _connections.Add(key, connections);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(key, out var connections))
            {
                return connections;
            }
        }

        return [];
    }

    public List<HashSet<string>> GetConnections(Func<object, bool> query)
    {
        lock (_connections)
        {
            return _connections
                .Where(x => query(x.Key))
                .Select(x => x.Value)
                .ToList();
        }
    }

    public void Remove(T key, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(key, out var connections))
            {
                return;
            }

            lock (connections)
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _connections.Remove(key);
                }
            }
        }
    }

    public void Remove(string connectionId)
    {
        lock (_connections)
        {
            foreach (var key in _connections.Keys)
            {
                if (!_connections.TryGetValue(key, out var connections))
                {
                    continue;
                }

                lock (connections)
                {
                    if (!connections.Remove(connectionId)) continue;
                    if (connections.Count == 0)
                    {
                        _connections.Remove(key);
                    }
                }
            }
        }
    }
}