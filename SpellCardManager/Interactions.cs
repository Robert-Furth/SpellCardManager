using ReactiveUI;
using SpellCardManager.Backend.Models;
using System.Reactive;

namespace SpellCardManager;

public static class Interactions {
    public static Interaction<SpellCard?, Unit> OpenSpellEditor { get; } = new();
    public static Interaction<Unit, Unit> OpenTagEditor { get; } = new();
}
