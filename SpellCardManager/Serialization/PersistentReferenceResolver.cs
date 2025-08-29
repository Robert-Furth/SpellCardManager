using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpellCardManager.Backend.Serialization;

/// <summary>
/// Mostly taken from <see href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references">this example</see>
/// </summary>
internal class PersistentReferenceResolver : ReferenceResolver {
    private uint _refcount = 0;
    private readonly Dictionary<string, object> _refToObj = [];
    private readonly Dictionary<object, string> _objToRef = new(ReferenceEqualityComparer.Instance);

    public override void AddReference(string referenceId, object value) {
        if (!_refToObj.TryAdd(referenceId, value)) throw new JsonException();
    }

    public override string GetReference(object value, out bool alreadyExists) {
        if (_objToRef.TryGetValue(value, out string? refId)) {
            alreadyExists = true;
        } else {
            _refcount++;
            refId = _refcount.ToString();
            _objToRef.Add(value, refId);
            alreadyExists = false;
        }

        return refId;
    }

    public override object ResolveReference(string referenceId) {
        if (!_refToObj.TryGetValue(referenceId, out object? value)) throw new JsonException();
        return value;
    }
}

internal class PersistentReferenceHandler : ReferenceHandler {

    private PersistentReferenceResolver? _resolver = null;

    public PersistentReferenceHandler() => Reset();

    public override ReferenceResolver CreateResolver() =>
        _resolver ?? throw new NullReferenceException();

    public void Reset() => _resolver = new();
}

