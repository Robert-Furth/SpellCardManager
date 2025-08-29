using ReactiveUI;
using System.Drawing;
using System.Text.Json.Serialization;

namespace SpellCardManager.Backend.Models;

public partial class Tag : ReactiveObject {
    public required string Name {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    private string _name = "";

    public Color Color {
        get => _color;
        set => this.RaiseAndSetIfChanged(ref _color, value);
    }
    private Color _color = Color.Black;

    [JsonIgnore]
    public Guid Id { get; init; } = Guid.NewGuid();

    public Tag Clone() => new() { Name = Name, Color = Color, Id = Id };

    public void Update(Tag other) {
        Name = other.Name;
        Color = other.Color;
    }
}

