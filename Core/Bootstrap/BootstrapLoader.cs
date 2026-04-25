// ============================================================
// BootstrapLoader.cs — Bailiff & Co
// Sur la scène Bootstrap (index 0 dans Build Settings).
// Attend que GameManager et SceneLoader soient initialisés
// (leur Awake() tourne en premier car même GameObject),
// puis charge la scène Menu.
//
// SETUP UNITY :
//   GameObject "GameManager" dans la scène Bootstrap :
//   ├── GameManager.cs
//   ├── SceneLoader.cs
//   └── BootstrapLoader.cs
// ============================================================
using System.Collections;
using UnityEngine;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Scène à charger après Bootstrap")]
    [SerializeField] private string _sceneDepart = SceneNames.MENU;

    [Tooltip("Délai en secondes avant de charger (0 = immédiat). Utile pour voir le splash screen.")]
    [SerializeField] private float _delaiDemarrage = 0f;

    private IEnumerator Start()
    {
        // Attend que tout soit initialisé
        yield return new WaitForSeconds(_delaiDemarrage);

        // Vérifie que les singletons sont bien là
        if (GameManager.Instance == null)
        {
            Debug.LogError("[Bootstrap] GameManager introuvable !");
            yield break;
        }
        if (SceneLoader.Instance == null)
        {
            Debug.LogError("[Bootstrap] SceneLoader introuvable !");
            yield break;
        }

        Debug.Log($"[Bootstrap] Démarrage → chargement de '{_sceneDepart}'");
        SceneLoader.Instance.ChargerScene(_sceneDepart, avecFondu: false);
    }
}
