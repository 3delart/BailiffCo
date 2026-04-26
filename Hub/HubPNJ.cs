// ============================================================
// HubPNJ.cs — Bailiff & Co
// À mettre sur chaque PNJ de l'agence dans le Hub.
// Implémente IInteractable — le joueur appuie E pour interagir.
//
// SETUP UNITY :
//   PNJ (Capsule ou FBX — Collider sur Layer Interactable)
//   ├── HubPNJ.cs
//   └── (optionnel) LabelCanvas World Space
//       └── TextMeshPro _labelTexte
//
//   Dans l'Inspector :
//   - _nomPnj      : "Chef", "Secrétaire", "Mécanicien"…
//   - _actionLabel : "Parler", "Boutique", "Garage"…
//   - _typePanneau : quel panneau ouvrir
//   - _debloque    : false = visible mais verrouillé (grisé)
//   - _conditionDeblocage : texte affiché si verrouillé
//
// TYPES DE PANNEAUX :
//   Missions   → Chef (toujours débloqué)
//   Boutique   → Secrétaire (toujours débloquée)
//   Inventaire → Table d'inventaire (toujours débloquée)
//   Garage     → Mécanicien (débloqué après mission 5)
//   Archiviste → Archiviste (débloqué après fin campagne)
// ============================================================
using TMPro;
using UnityEngine;

public class HubPNJ : MonoBehaviour, IInteractable
{
    // ================================================================
    // TYPES
    // ================================================================

    public enum TypePanneau
    {
        Missions,
        Boutique,
        Inventaire,
        Garage,
        Archiviste,
    }

    // ================================================================
    // SÉRIALISATION
    // ================================================================

    [Header("Identité")]
    [SerializeField] private string      _nomPnj             = "Chef";
    [SerializeField] private string      _actionLabel        = "Parler";
    [SerializeField] private TypePanneau _typePanneau        = TypePanneau.Missions;

    [Header("Déblocage")]
    [SerializeField] private bool        _debloque           = true;
    [SerializeField] private string      _conditionDeblocage = "Terminer la campagne";

    [Header("Label flottant (optionnel)")]
    [SerializeField] private TextMeshPro _labelTexte;
    [SerializeField] private float       _hauteurLabel       = 2.2f;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Awake()
    {
        if (_labelTexte != null)
        {
            _labelTexte.transform.localPosition = Vector3.up * _hauteurLabel;
            MettreAJourLabel();
        }
    }

    private void Update()
    {
        // Billboard — label face à la caméra
        if (_labelTexte != null && Camera.main != null)
        {
            _labelTexte.transform.LookAt(
                _labelTexte.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    // ================================================================
    // IINTERACTABLE
    // ================================================================

    public bool CanInteract(GameObject interacteur) => true; // toujours — pour afficher le label

    public void Interact(GameObject interacteur)
    {
        if (!_debloque)
        {
            FindObjectOfType<HubUI>()?.AfficherErreur(
                $"{_nomPnj} — Verrouillé\n{_conditionDeblocage}");
            return;
        }

        var ui = FindObjectOfType<HubUI>();
        if (ui == null)
        {
            Debug.LogWarning($"[HubPNJ] {_nomPnj} : HubUI introuvable !");
            return;
        }

        switch (_typePanneau)
        {
            case TypePanneau.Missions:    ui.OuvrirPanelMissions();    break;
            case TypePanneau.Boutique:    ui.OuvrirPanelBoutique();    break;
            case TypePanneau.Inventaire:  ui.OuvrirPanelInventaire();  break;
            case TypePanneau.Garage:      ui.OuvrirPanelGarage();      break;
            case TypePanneau.Archiviste:  ui.OuvrirPanelMissions();    break; // même panel, missions libres
        }
    }

    public string GetInteractionLabel()
    {
        if (!_debloque)
            return $"{_nomPnj} — 🔒 {_conditionDeblocage}";

        return $"{_nomPnj} — [E] {_actionLabel}";
    }

    // ================================================================
    // API PUBLIQUE — appelé par SaveSystem au chargement
    // ================================================================

    public void Debloquer()
    {
        _debloque = true;
        MettreAJourLabel();
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private void MettreAJourLabel()
    {
        if (_labelTexte == null) return;

        _labelTexte.text = _debloque
            ? $"{_nomPnj}\n<size=70%>[E] {_actionLabel}</size>"
            : $"{_nomPnj}\n<size=70%>🔒 {_conditionDeblocage}</size>";

        // Grise le label si verrouillé
        _labelTexte.color = _debloque
            ? Color.white
            : new Color(0.5f, 0.5f, 0.5f, 1f);
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public bool   EstDebloque  => _debloque;
    public string NomPnj       => _nomPnj;
}
