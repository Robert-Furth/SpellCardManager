using DynamicData;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpellCardManager.Backend.Serialization;

internal class SourceCacheJsonConverter<TObject, TKey>
    : JsonConverter<SourceCache<TObject, TKey>>
    where TObject : notnull
    where TKey : notnull {

    private readonly Func<TObject, TKey> _keySelector;

    public SourceCacheJsonConverter(Func<TObject, TKey> keySelector) {
        _keySelector = keySelector;
    }

    public override SourceCache<TObject, TKey>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {

        var list = JsonSerializer.Deserialize<List<TObject>>(ref reader, options);
        if (list is null) return null;

        var cache = new SourceCache<TObject, TKey>(_keySelector);
        cache.AddOrUpdate(list);
        return cache;
    }

    public override void Write(
        Utf8JsonWriter writer,
        SourceCache<TObject, TKey> value,
        JsonSerializerOptions options) {

        var objectConverter = (JsonConverter<TObject>)options.GetConverter(typeof(TObject));

        writer.WriteStartArray();
        foreach (var item in value.Items) {
            objectConverter.Write(writer, item, options);
        }
        writer.WriteEndArray();
    }
}
