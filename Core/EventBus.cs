// ============================================================
// EventBus.cs — Bailiff & Co
// Bus d'événements central. RÈGLE ABSOLUE : aucun système
// n'appelle directement un autre. Toute communication passe ici.
// Usage : EventBus<OnObjetCharge>.Raise(new OnObjetCharge(...));
//         EventBus<OnObjetCharge>.Subscribe(Handler);
// ============================================================
using System;
using System.Collections.Generic;

public static class EventBus<T> where T : struct
{
    private static readonly List<Action<T>> _handlers = new();

    public static void Subscribe(Action<T> handler)
    {
        if (!_handlers.Contains(handler))
            _handlers.Add(handler);
    }

    public static void Unsubscribe(Action<T> handler)
    {
        _handlers.Remove(handler);
    }

    public static void Raise(T evt)
    {
        // Copie pour éviter les modifications pendant l'itération
        for (int i = _handlers.Count - 1; i >= 0; i--)
            _handlers[i]?.Invoke(evt);
    }

    /// <summary>Nettoyage en fin de mission — évite les fuites mémoire.</summary>
    public static void Clear() => _handlers.Clear();
}
