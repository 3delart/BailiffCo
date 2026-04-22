// DebugClics.cs — Debug temporaire
// Attacher sur le GameObject Canvas de la scène Menu.
// Affiche dans la Console tout ce qui se passe avec les clics.
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DebugClics : MonoBehaviour
{
    private void Update()
    {
        // ── 1. Clic souris détecté ?
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"[DebugClics] 🖱️ Clic gauche détecté à {Input.mousePosition}");

            // ── 2. EventSystem présent ?
            if (EventSystem.current == null)
            {
                Debug.LogError("[DebugClics] ❌ Aucun EventSystem actif dans la scène !");
                return;
            }

            Debug.Log($"[DebugClics] ✅ EventSystem actif : {EventSystem.current.gameObject.name} " +
                      $"(scène : {EventSystem.current.gameObject.scene.name})");

            // ── 3. Combien d'EventSystems existent ?
            var allES = FindObjectsOfType<EventSystem>();
            if (allES.Length > 1)
            {
                Debug.LogWarning($"[DebugClics] ⚠️ {allES.Length} EventSystems trouvés ! " +
                                 "Les clics peuvent être avalés par le mauvais.");
                foreach (var es in allES)
                    Debug.LogWarning($"   → {es.gameObject.name} dans '{es.gameObject.scene.name}'");
            }

            // ── 4. Quel objet UI est sous le curseur ?
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            if (results.Count == 0)
            {
                Debug.LogWarning("[DebugClics] ⚠️ Aucun objet UI détecté sous le curseur. " +
                                 "Vérifie que le Canvas a un GraphicRaycaster.");
            }
            else
            {
                Debug.Log($"[DebugClics] 🎯 {results.Count} objet(s) UI sous le curseur :");
                foreach (var r in results)
                    Debug.Log($"   → {r.gameObject.name} | " +
                              $"Interactable: {EstInteractable(r.gameObject)} | " +
                              $"Depth: {r.depth} | " +
                              $"Scène: {r.gameObject.scene.name}");
            }

            // ── 5. Le Canvas a-t-il un GraphicRaycaster ?
            var canvases = FindObjectsOfType<Canvas>();
            foreach (var c in canvases)
            {
                var gr = c.GetComponent<GraphicRaycaster>();
                Debug.Log($"[DebugClics] Canvas '{c.name}' — " +
                          $"GraphicRaycaster: {(gr != null ? "✅" : "❌ MANQUANT")} | " +
                          $"Enabled: {c.enabled} | " +
                          $"RenderMode: {c.renderMode}");
            }
        }
    }

    private static string EstInteractable(GameObject go)
    {
        var selectable = go.GetComponent<Selectable>();
        if (selectable == null) return "N/A";
        return selectable.interactable ? "✅ oui" : "❌ non";
    }
}
