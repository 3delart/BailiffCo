// ============================================================
// CustomizationUI.cs — Bailiff & Co
// UI de personnalisation : 4 onglets principaux,
// 12 sous-catégories, flèches gauche/droite, grilles couleurs.
//
// ── SETUP CANVAS (Screen Space Overlay) ──────────────────────
//
//  [PanneauPrincipal]
//  ├── ZonePreview (RawImage → RenderTexture de la caméra perso)
//  │
//  ├── [BarreOnglets]  ← 4 boutons principaux
//  │   ├── BtnOngletVisage
//  │   ├── BtnOngletCheveux
//  │   ├── BtnOngletTenue
//  │   └── BtnOngletPeau
//  │
//  ├── [PanneauVisage]   (activé quand onglet Visage sélectionné)
//  │   ├── [BarreSousOnglets]
//  │   │   ├── BtnYeux / BtnNez / BtnBouche / BtnSourceils / BtnBarbe
//  │   ├── BtnGauche  [◄]
//  │   ├── TexteNomOption (TMP)
//  │   ├── TexteIndex "1 / 5" (TMP)
//  │   ├── BtnDroite  [►]
//  │   └── GridIcones (grille de boutons icônes optionnelle)
//  │
//  ├── [PanneauCheveux]
//  │   ├── [BarreSousOnglets]
//  │   │   ├── BtnCoiffure / BtnCouleurCheveux
//  │   ├── BtnGauche / TexteNomOption / TexteIndex / BtnDroite
//  │   └── GridCouleurs (pastilles, visible pour CouleurCheveux)
//  │
//  ├── [PanneauTenue]
//  │   ├── [BarreSousOnglets]
//  │   │   ├── BtnTenue / BtnChapeau / BtnAccessoire
//  │   ├── BtnGauche / TexteNomOption / TexteIndex / BtnDroite
//  │
//  ├── [PanneauPeau]
//  │   └── GridCouleursPeau (pastilles)
//  │
//  └── [BasDePage]
//      ├── BtnConfirmer
//      └── BtnAnnuler
//
// CHAQUE PANNEAU a ses propres références assignées dans l'Inspector.
// ============================================================
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationUI : MonoBehaviour
{
    // ================================================================
    // ONGLETS PRINCIPAUX
    // ================================================================

    [Header("Référence Preview")]
    [SerializeField] private CharacterPreviewController _preview;

    [Header("Boutons onglets principaux")]
    [SerializeField] private Button _btnOngletVisage;
    [SerializeField] private Button _btnOngletCheveux;
    [SerializeField] private Button _btnOngletTenue;
    [SerializeField] private Button _btnOngletPeau;

    [Header("Panneaux principaux (un actif à la fois)")]
    [SerializeField] private GameObject _panneauVisage;
    [SerializeField] private GameObject _panneauCheveux;
    [SerializeField] private GameObject _panneauTenue;
    [SerializeField] private GameObject _panneauPeau;

    // ================================================================
    // SOUS-ONGLETS VISAGE
    // ================================================================

    [Header("Sous-onglets Visage")]
    [SerializeField] private Button _btnYeux;
    [SerializeField] private Button _btnNez;
    [SerializeField] private Button _btnBouche;
    [SerializeField] private Button _btnSourceils;
    [SerializeField] private Button _btnBarbe;

    // ================================================================
    // SOUS-ONGLETS CHEVEUX
    // ================================================================

    [Header("Sous-onglets Cheveux")]
    [SerializeField] private Button _btnCoiffure;
    [SerializeField] private Button _btnCouleurCheveux;

    // ================================================================
    // SOUS-ONGLETS TENUE
    // ================================================================

    [Header("Sous-onglets Tenue")]
    [SerializeField] private Button _btnTenue;
    [SerializeField] private Button _btnChapeau;
    [SerializeField] private Button _btnAccessoire;

    // ================================================================
    // PANNEAU OPTION PARTAGÉ (flèches + label)
    // ================================================================

    [Header("Sélecteur d'option (flèches)")]
    [SerializeField] private Button          _btnGauche;
    [SerializeField] private Button          _btnDroite;
    [SerializeField] private TextMeshProUGUI _texteNomOption;
    [SerializeField] private TextMeshProUGUI _texteIndex;   // "2 / 6"
    [SerializeField] private Image           _iconeOption;  // icône de l'option courante

    // ================================================================
    // GRILLES DE COULEURS
    // ================================================================

    [Header("Grille couleurs (pastilles)")]
    [SerializeField] private Transform   _gridCouleursPeau;
    [SerializeField] private Transform   _gridCouleursCheveux;
    [SerializeField] private GameObject  _prefabPastilleCouleur; // bouton rond coloré

    [Header("Sélecteur d'option masqué quand grille couleur visible")]
    [SerializeField] private GameObject _panneauFlechesOption;

    // ================================================================
    // BAS DE PAGE
    // ================================================================

    [Header("Bas de page")]
    [SerializeField] private Button _btnConfirmer;
    [SerializeField] private Button _btnAnnuler;

    // ================================================================
    // STYLE ONGLETS
    // ================================================================

    [Header("Style onglets")]
    [SerializeField] private Color _couleurActif   = new Color(0.20f, 0.55f, 1.00f);
    [SerializeField] private Color _couleurInactif = new Color(0.12f, 0.12f, 0.18f);

    // ================================================================
    // ÉTAT INTERNE
    // ================================================================

    private enum OngletPrincipal { Visage, Cheveux, Tenue, Peau }

    private enum SousOnglet
    {
        // Visage
        Yeux, Nez, Bouche, Sourcils, Barbe,
        // Cheveux
        Coiffure, CouleurCheveux,
        // Tenue
        Tenue, Chapeau, Accessoire
    }

    private OngletPrincipal _ongletPrincipal = OngletPrincipal.Visage;
    private SousOnglet      _sousOnglet       = SousOnglet.Yeux;

    private CharacterCustomizationData _dataTemp;
    private CharacterCustomizationDef  _def;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        _def = _preview?.Def;

        // Copie de travail
        _dataTemp = GameManager.Instance != null
            ? GameManager.Instance.Personnalisation.Clone()
            : new CharacterCustomizationData();

        BrancherOngletsPrincipaux();
        BrancherSousOnglets();
        BrancherFleches();
        BrancherBasDePage();

        ConstruireGrilleCouleurs(_gridCouleursPeau,    _def?.CouleursPeau,    OnCouleurPeauChoisie);
        ConstruireGrilleCouleurs(_gridCouleursCheveux, _def?.CouleursCheveux, OnCouleurCheveuxChoisie);

        OuvrirOnglet(OngletPrincipal.Visage, SousOnglet.Yeux);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ================================================================
    // BRANCHEMENT BOUTONS
    // ================================================================

    private void BrancherOngletsPrincipaux()
    {
        _btnOngletVisage?.onClick.AddListener(()  => OuvrirOnglet(OngletPrincipal.Visage,  SousOnglet.Yeux));
        _btnOngletCheveux?.onClick.AddListener(() => OuvrirOnglet(OngletPrincipal.Cheveux, SousOnglet.Coiffure));
        _btnOngletTenue?.onClick.AddListener(()   => OuvrirOnglet(OngletPrincipal.Tenue,   SousOnglet.Tenue));
        _btnOngletPeau?.onClick.AddListener(()    => OuvrirOnglet(OngletPrincipal.Peau,    SousOnglet.Yeux));
    }

    private void BrancherSousOnglets()
    {
        // Visage
        _btnYeux?.onClick.AddListener(()      => ChangerSousOnglet(SousOnglet.Yeux));
        _btnNez?.onClick.AddListener(()       => ChangerSousOnglet(SousOnglet.Nez));
        _btnBouche?.onClick.AddListener(()    => ChangerSousOnglet(SousOnglet.Bouche));
        _btnSourceils?.onClick.AddListener(() => ChangerSousOnglet(SousOnglet.Sourcils));
        _btnBarbe?.onClick.AddListener(()     => ChangerSousOnglet(SousOnglet.Barbe));
        // Cheveux
        _btnCoiffure?.onClick.AddListener(()       => ChangerSousOnglet(SousOnglet.Coiffure));
        _btnCouleurCheveux?.onClick.AddListener(() => ChangerSousOnglet(SousOnglet.CouleurCheveux));
        // Tenue
        _btnTenue?.onClick.AddListener(()     => ChangerSousOnglet(SousOnglet.Tenue));
        _btnChapeau?.onClick.AddListener(()   => ChangerSousOnglet(SousOnglet.Chapeau));
        _btnAccessoire?.onClick.AddListener(() => ChangerSousOnglet(SousOnglet.Accessoire));
    }

    private void BrancherFleches()
    {
        _btnGauche?.onClick.AddListener(PrecedentOption);
        _btnDroite?.onClick.AddListener(SuivantOption);
    }

    private void BrancherBasDePage()
    {
        _btnConfirmer?.onClick.AddListener(Confirmer);
        _btnAnnuler?.onClick.AddListener(Annuler);
    }

    // ================================================================
    // NAVIGATION ONGLETS
    // ================================================================

    private void OuvrirOnglet(OngletPrincipal onglet, SousOnglet defautSousOnglet)
    {
        _ongletPrincipal = onglet;

        // Affiche le bon panneau principal
        _panneauVisage?.SetActive(onglet == OngletPrincipal.Visage);
        _panneauCheveux?.SetActive(onglet == OngletPrincipal.Cheveux);
        _panneauTenue?.SetActive(onglet == OngletPrincipal.Tenue);
        _panneauPeau?.SetActive(onglet == OngletPrincipal.Peau);

        // Colorie les onglets principaux
        SetCouleurOnglet(_btnOngletVisage,  onglet == OngletPrincipal.Visage);
        SetCouleurOnglet(_btnOngletCheveux, onglet == OngletPrincipal.Cheveux);
        SetCouleurOnglet(_btnOngletTenue,   onglet == OngletPrincipal.Tenue);
        SetCouleurOnglet(_btnOngletPeau,    onglet == OngletPrincipal.Peau);

        // La peau n'a pas de sous-onglets ni de flèches — juste la grille
        bool estPeau = onglet == OngletPrincipal.Peau;
        _panneauFlechesOption?.SetActive(!estPeau);
        _gridCouleursPeau?.gameObject.SetActive(estPeau);

        if (!estPeau)
            ChangerSousOnglet(defautSousOnglet);
    }

    private void ChangerSousOnglet(SousOnglet sousOnglet)
    {
        _sousOnglet = sousOnglet;
        MettreAJourCouleursSousOnglets();

        // Affiche/masque la grille couleurs cheveux
        bool estCouleurCheveux = sousOnglet == SousOnglet.CouleurCheveux;
        _gridCouleursCheveux?.gameObject.SetActive(estCouleurCheveux);
        _panneauFlechesOption?.SetActive(!estCouleurCheveux &&
            _ongletPrincipal != OngletPrincipal.Peau);

        MettreAJourAffichageOption();
    }

    private void MettreAJourCouleursSousOnglets()
    {
        // Visage
        SetCouleurOnglet(_btnYeux,      _sousOnglet == SousOnglet.Yeux);
        SetCouleurOnglet(_btnNez,       _sousOnglet == SousOnglet.Nez);
        SetCouleurOnglet(_btnBouche,    _sousOnglet == SousOnglet.Bouche);
        SetCouleurOnglet(_btnSourceils, _sousOnglet == SousOnglet.Sourcils);
        SetCouleurOnglet(_btnBarbe,     _sousOnglet == SousOnglet.Barbe);
        // Cheveux
        SetCouleurOnglet(_btnCoiffure,       _sousOnglet == SousOnglet.Coiffure);
        SetCouleurOnglet(_btnCouleurCheveux, _sousOnglet == SousOnglet.CouleurCheveux);
        // Tenue
        SetCouleurOnglet(_btnTenue,      _sousOnglet == SousOnglet.Tenue);
        SetCouleurOnglet(_btnChapeau,    _sousOnglet == SousOnglet.Chapeau);
        SetCouleurOnglet(_btnAccessoire, _sousOnglet == SousOnglet.Accessoire);
    }

    private void SetCouleurOnglet(Button btn, bool actif)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img) img.color = actif ? _couleurActif : _couleurInactif;
    }

    // ================================================================
    // FLÈCHES — PRÉCÉDENT / SUIVANT
    // ================================================================

    private void PrecedentOption() => ChangerIndex(-1);
    private void SuivantOption()   => ChangerIndex(+1);

    private void ChangerIndex(int delta)
    {
        int max = GetMaxIndex();
        if (max <= 0) return;

        switch (_sousOnglet)
        {
            case SousOnglet.Yeux:
                _dataTemp.IndexYeux = Mod(_dataTemp.IndexYeux + delta, max);
                _preview.AppliquerYeux(_dataTemp.IndexYeux);
                break;
            case SousOnglet.Nez:
                _dataTemp.IndexNez = Mod(_dataTemp.IndexNez + delta, max);
                _preview.AppliquerNez(_dataTemp.IndexNez);
                break;
            case SousOnglet.Bouche:
                _dataTemp.IndexBouche = Mod(_dataTemp.IndexBouche + delta, max);
                _preview.AppliquerBouche(_dataTemp.IndexBouche);
                break;
            case SousOnglet.Sourcils:
                _dataTemp.IndexSourceils = Mod(_dataTemp.IndexSourceils + delta, max);
                _preview.AppliquerSourceils(_dataTemp.IndexSourceils);
                break;
            case SousOnglet.Barbe:
                _dataTemp.IndexBarbe = Mod(_dataTemp.IndexBarbe + delta, max);
                _preview.AppliquerBarbe(_dataTemp.IndexBarbe);
                break;
            case SousOnglet.Coiffure:
                _dataTemp.IndexCoiffure = Mod(_dataTemp.IndexCoiffure + delta, max);
                _preview.AppliquerCoiffure(_dataTemp.IndexCoiffure);
                break;
            case SousOnglet.Tenue:
                _dataTemp.IndexTenue = Mod(_dataTemp.IndexTenue + delta, max);
                _preview.AppliquerTenue(_dataTemp.IndexTenue);
                break;
            case SousOnglet.Chapeau:
                _dataTemp.IndexChapeau = Mod(_dataTemp.IndexChapeau + delta, max);
                _preview.AppliquerChapeau(_dataTemp.IndexChapeau);
                break;
            case SousOnglet.Accessoire:
                _dataTemp.IndexAccessoire = Mod(_dataTemp.IndexAccessoire + delta, max);
                _preview.AppliquerAccessoire(_dataTemp.IndexAccessoire);
                break;
        }

        MettreAJourAffichageOption();
    }

    // ================================================================
    // COULEURS PAR GRILLE (pastilles)
    // ================================================================

    private void ConstruireGrilleCouleurs(Transform grid, Color[] couleurs, System.Action<int> callback)
    {
        if (grid == null || couleurs == null || _prefabPastilleCouleur == null) return;

        // Nettoie les anciens
        foreach (Transform enfant in grid)
            Destroy(enfant.gameObject);

        for (int i = 0; i < couleurs.Length; i++)
        {
            int idx = i; // capture pour la closure
            GameObject go  = Instantiate(_prefabPastilleCouleur, grid);
            var img = go.GetComponent<Image>();
            if (img) img.color = couleurs[i];
            var btn = go.GetComponent<Button>();
            btn?.onClick.AddListener(() => callback(idx));
        }
    }

    private void OnCouleurPeauChoisie(int index)
    {
        _dataTemp.IndexCouleurPeau = index;
        _preview.AppliquerCouleurPeau(index);
    }

    private void OnCouleurCheveuxChoisie(int index)
    {
        _dataTemp.IndexCouleurCheveux = index;
        _preview.AppliquerCouleurCheveux(index);
    }

    // ================================================================
    // AFFICHAGE DE L'OPTION COURANTE
    // ================================================================

    private void MettreAJourAffichageOption()
    {
        if (_def == null) return;

        string nom   = "—";
        Sprite icone = null;
        int    idx   = 0;
        int    max   = 0;

        switch (_sousOnglet)
        {
            case SousOnglet.Yeux:
                idx = _dataTemp.IndexYeux; max = _def.Yeux?.Length ?? 0;
                if (max > 0) { nom = _def.Yeux[idx].Nom; icone = _def.Yeux[idx].IconeUI; }
                break;
            case SousOnglet.Nez:
                idx = _dataTemp.IndexNez; max = _def.Nez?.Length ?? 0;
                if (max > 0) { nom = _def.Nez[idx].Nom; icone = _def.Nez[idx].IconeUI; }
                break;
            case SousOnglet.Bouche:
                idx = _dataTemp.IndexBouche; max = _def.Bouches?.Length ?? 0;
                if (max > 0) { nom = _def.Bouches[idx].Nom; icone = _def.Bouches[idx].IconeUI; }
                break;
            case SousOnglet.Sourcils:
                idx = _dataTemp.IndexSourceils; max = _def.Sourcils?.Length ?? 0;
                if (max > 0) { nom = _def.Sourcils[idx].Nom; icone = _def.Sourcils[idx].IconeUI; }
                break;
            case SousOnglet.Barbe:
                idx = _dataTemp.IndexBarbe; max = _def.Barbes?.Length ?? 0;
                if (max > 0) { nom = _def.Barbes[idx].Nom; icone = _def.Barbes[idx].IconeUI; }
                break;
            case SousOnglet.Coiffure:
                idx = _dataTemp.IndexCoiffure; max = _def.Coiffures?.Length ?? 0;
                if (max > 0) { nom = _def.Coiffures[idx].Nom; icone = _def.Coiffures[idx].IconeUI; }
                break;
            case SousOnglet.CouleurCheveux:
                idx = _dataTemp.IndexCouleurCheveux; max = _def.CouleursCheveux?.Length ?? 0;
                nom = max > 0 ? $"Couleur {idx + 1}" : "—";
                break;
            case SousOnglet.Tenue:
                idx = _dataTemp.IndexTenue; max = _def.Tenues?.Length ?? 0;
                if (max > 0) { nom = _def.Tenues[idx].Nom; icone = _def.Tenues[idx].IconeUI; }
                break;
            case SousOnglet.Chapeau:
                idx = _dataTemp.IndexChapeau; max = _def.Chapeaux?.Length ?? 0;
                if (max > 0) { nom = _def.Chapeaux[idx].Nom; icone = _def.Chapeaux[idx].IconeUI; }
                break;
            case SousOnglet.Accessoire:
                idx = _dataTemp.IndexAccessoire; max = _def.Accessoires?.Length ?? 0;
                if (max > 0) { nom = _def.Accessoires[idx].Nom; icone = _def.Accessoires[idx].IconeUI; }
                break;
        }

        if (_texteNomOption) _texteNomOption.text = nom;
        if (_texteIndex)     _texteIndex.text     = max > 0 ? $"{idx + 1} / {max}" : "—";
        if (_iconeOption)
        {
            _iconeOption.sprite = icone;
            _iconeOption.gameObject.SetActive(icone != null);
        }
    }

    // ================================================================
    // CONFIRMER / ANNULER
    // ================================================================

    private void Confirmer()
    {
        GameManager.Instance?.SauvegarderPersonnalisation(_dataTemp);
        SceneLoader.Instance?.ChargerScene(SceneNames.MENU, avecFondu: true);
    }

    private void Annuler()
    {
        // Remet le preview à l'état original sans sauvegarder
        if (GameManager.Instance != null)
            _preview.AppliquerTout(GameManager.Instance.Personnalisation);
        SceneLoader.Instance?.ChargerScene(SceneNames.MENU, avecFondu: true);
    }

    // ================================================================
    // UTILITAIRES
    // ================================================================

    private int GetMaxIndex()
    {
        if (_def == null) return 0;
        return _sousOnglet switch
        {
            SousOnglet.Yeux           => _def.Yeux?.Length       ?? 0,
            SousOnglet.Nez            => _def.Nez?.Length        ?? 0,
            SousOnglet.Bouche         => _def.Bouches?.Length    ?? 0,
            SousOnglet.Sourcils       => _def.Sourcils?.Length   ?? 0,
            SousOnglet.Barbe          => _def.Barbes?.Length     ?? 0,
            SousOnglet.Coiffure       => _def.Coiffures?.Length  ?? 0,
            SousOnglet.CouleurCheveux => _def.CouleursCheveux?.Length ?? 0,
            SousOnglet.Tenue          => _def.Tenues?.Length     ?? 0,
            SousOnglet.Chapeau        => _def.Chapeaux?.Length   ?? 0,
            SousOnglet.Accessoire     => _def.Accessoires?.Length ?? 0,
            _                         => 0
        };
    }

    /// <summary>Modulo positif — permet de cycler 0 → n-1 → 0 dans les deux sens.</summary>
    private static int Mod(int x, int m) => m <= 0 ? 0 : ((x % m) + m) % m;
}
