// ============================================================
// LabelInteractionUI.cs — Bailiff & Co
// Affiche le label d'interaction contextuel.
// Le panel reste TOUJOURS actif — on vide le texte quand
// il n'y a rien à afficher.
// La touche affichee est toujours lue depuis OptionsManager
// pour refleter les rebinds du joueur.
// ============================================================
using TMPro;
using UnityEngine;

public class LabelInteractionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _txtTouche;
    [SerializeField] private TextMeshProUGUI _txtAction;

    private PlayerInteractor _interactor;

    private void Awake()
    {
        _interactor = FindObjectOfType<PlayerInteractor>();
    }

    private void Update()
    {
        if (_interactor == null)
        {
            _interactor = FindObjectOfType<PlayerInteractor>();
            if (_interactor == null) return;
        }

        string label = _interactor.GetLabelCourant();

        if (string.IsNullOrEmpty(label))
        {
            if (_txtTouche != null) _txtTouche.text = "";
            if (_txtAction != null) _txtAction.text = "";
            return;
        }

        ParseEtAfficher(label);
    }

    private void ParseEtAfficher(string label)
    {
        // Touche reelle depuis OptionsManager (tient compte des rebinds)
        string toucheReelle = GetToucheInteragir();

        int debut = label.IndexOf('[');
        int fin   = label.IndexOf(']');

        if (debut >= 0 && fin > debut)
        {
            // Le label contient [X] — on remplace X par la vraie touche rebindee
            string action = label.Substring(fin + 1).Trim();

            if (action.StartsWith("—") || action.StartsWith("-"))
                action = action.Substring(1).Trim();

            if (_txtTouche != null) _txtTouche.text = toucheReelle;
            if (_txtAction != null) _txtAction.text = action;
        }
        else
        {
            // Pas de [X] dans le label — affiche la vraie touche quand meme
            if (_txtTouche != null) _txtTouche.text = toucheReelle;
            if (_txtAction != null) _txtAction.text = label;
        }
    }

    private string GetToucheInteragir()
    {
        if (OptionsManager.Instance == null)
            return "E"; // fallback si OptionsManager absent

        KeyCode kc = OptionsManager.Instance.GetTouche(ActionJeu.Interagir);
        return KeyRebindUI.FormatKeyCode(kc);
    }
}
