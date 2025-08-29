using DynamicData;
using SpellCardManager.Backend.Serialization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace SpellCardManager.Backend.Models;

public class CardCollection {
    public SourceCache<Tag, Guid> Tags { get; init; } = new(x => x.Id);

    public SourceCache<SpellCard, Guid> Cards { get; init; } = new(x => x.Id);


    public void RemoveCard(Guid cardId) => Cards.RemoveKey(cardId);

    public void RemoveCard(SpellCard card) => Cards.Remove(card);

    public bool HasCardId(Guid id) => Cards.Lookup(id).HasValue;

    public bool HasCard(SpellCard card) => HasCardId(card.Id);


    public void RemoveTag(Guid tagId) {
        var optional = Tags.Lookup(tagId);
        if (optional.HasValue) {
            RemoveTag(optional.Value);
        }
    }

    private void RemoveTag(Tag tag) {
        Tags.Remove(tag);
        foreach (var card in Cards.Items) {
            card.Tags.Remove(tag);
        }
    }

    #region JSON

    private static readonly PersistentReferenceHandler _refHandler = new();

    private static readonly JsonSerializerOptions _serializerOptions = new() {
        Converters = {
                new ColorJsonConverter(),
                new SourceCacheJsonConverter<SpellCard, Guid>(card => card.Id),
                new SourceCacheJsonConverter<Tag, Guid>(tag => tag.Id)
            },
        ReferenceHandler = _refHandler
    };

    private static readonly JsonSerializerOptions _serializerOptionsPretty =
        new(_serializerOptions) { WriteIndented = true };

    public string ToJson() {
        _refHandler.Reset();
        return JsonSerializer.Serialize(this, _serializerOptions);
    }

    public string ToJsonPretty() {
        _refHandler.Reset();
        return JsonSerializer.Serialize(this, _serializerOptionsPretty);
    }

    public static CardCollection? FromJson(string text) {
        _refHandler.Reset();
        return JsonSerializer.Deserialize<CardCollection>(text, _serializerOptions);
    }

    public static CardCollection? FromJson(Stream stream) {
        _refHandler.Reset();
        return JsonSerializer.Deserialize<CardCollection>(stream, _serializerOptions);
    }

    #endregion

    #region Save/Load

    public void WriteUncompressed(Stream stream) {
        using var writer = new StreamWriter(stream);
        writer.Write(ToJson());
    }

    public void WriteCompressed(Stream stream) {
        var jsonBytes = Encoding.UTF8.GetBytes(ToJson());

        using (var writer = new BinaryWriter(stream, encoding: Encoding.UTF8, true)) {
            // 0x00 D E C K v 0x01 0x00 (deck version 1)
            writer.Write("\0DECKv\x01\x00".AsSpan());
            // Length of uncompressed data
            writer.Write(jsonBytes.LongLength);
        }

        using var compressor = new DeflateStream(stream, CompressionMode.Compress);
        compressor.Write(Encoding.UTF8.GetBytes(ToJson()));
    }

    #endregion
}

