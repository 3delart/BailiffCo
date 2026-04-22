// ============================================================
// MenuUI.cs — Bailiff & Co
// Gère les boutons du menu principal.
// À attacher sur le Canvas de la scène Menu.
//
// SETUP UNITY :
//   Attacher sur le Canvas, puis glisser chaque bouton
//   dans les champs correspondants de l'Inspector.
// ============================================================
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("Boutons")]
    [SerializeField] private Button _boutonJouer;
    [SerializeField] private Button _boutonCoop;
    [SerializeField] private Button _boutonOptions;
    [SerializeField] private Button _boutonQuitter;

    [Header("Panneaux")]
    [Tooltip("Panneau Options — désactivé par défaut, activé sur clic Options")]
    [SerializeField] private GameObject _panneauOptions;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        // Branche les boutons
        _boutonJouer?.onClick.AddListener(OnJouer);
        _boutonCoop?.onClick.AddListener(OnCoop);
        _boutonOptions?.onClick.AddListener(OnOptions);
        _boutonQuitter?.onClick.AddListener(OnQuitter);

        // Coop désactivé en V1
        if (_boutonCoop != null)
            _boutonCoop.interactable = false;

        // Options désactivées en V1
        if (_boutonOptions != null)
            _boutonOptions.interactable = false;

        // Panneau options fermé au départ
        if (_panneauOptions != null)
            _panneauOptions.SetActive(false);
    }

    private void OnDestroy()
    {
        // Nettoyage — bonne pratique même si la scène est détruite
        _boutonJouer?.onClick.RemoveListener(OnJouer);
        _boutonCoop?.onClick.RemoveListener(OnCoop);
        _boutonOptions?.onClick.RemoveListener(OnOptions);
        _boutonQuitter?.onClick.RemoveListener(OnQuitter);
    }

    // ================================================================
    // HANDLERS
    // ================================================================

    private void OnJouer()
    {
        // Solo → aller au Hub
        GameManager.Instance?.AllerAuHub();
    }

    private void OnCoop()
    {
        // TODO V2 : lancer le lobby multijoueur
        Debug.Log("[Menu] Coop — non implémenté en V1");
    }

    private void OnOptions()
    {
        if (_panneauOptions != null)
            _panneauOptions.SetActive(!_panneauOptions.activeSelf);
    }

    private void OnQuitter()
    {
        GameManager.Instance?.QuitterJeu();
    }
}
