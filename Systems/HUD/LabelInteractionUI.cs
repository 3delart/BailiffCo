// ============================================================
// LabelInteractionUI.cs — Bailiff & Co
// Affiche le label d'interaction contextuel.
// Le panel reste TOUJOURS actif — on vide le texte quand
// il n'y a rien à afficher.
// ============================================================
using TMPro;
using UnityEngine;

public class LabelInteractionUI : MonoBehaviour
{
    [Header("Références")]
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
            // Vide les textes — panel reste visible mais vide
            if (_txtTouche != null) _txtTouche.text = "";
            if (_txtAction != null) _txtAction.text = "";
            return;
        }

        ParseEtAfficher(label);
    }

    private void ParseEtAfficher(string label)
    {
        int debut = label.IndexOf('[');
        int fin   = label.IndexOf(']');

        if (debut >= 0 && fin > debut)
        {
            string touche = label.Substring(debut + 1, fin - debut - 1);
            string action = label.Substring(fin + 1).Trim();

            if (action.StartsWith("—") || action.StartsWith("-"))
                action = action.Substring(1).Trim();

            if (_txtTouche != null) _txtTouche.text = touche;
            if (_txtAction != null) _txtAction.text = action;
        }
        else
        {
            if (_txtTouche != null) _txtTouche.text = "E";
            if (_txtAction != null) _txtAction.text = label;
        }
    }
}
