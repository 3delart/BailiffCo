// ============================================================
// CharacterPreviewController.cs — Bailiff & Co
// Gère le mannequin 3D dans la scène CharacterCustomization.
// Applique tous les choix visuels en temps réel.
// Rotation orbit par drag souris + auto-rotation douce.
//
// HIÉRARCHIE RECOMMANDÉE dans la scène :
//
//   PreviewRoot  ← CE SCRIPT ici
//   └── Mannequin
//       ├── Body_SkinnedMeshRenderer   (corps + tenue, plusieurs slots matériaux)
//       ├── Face_SkinnedMeshRenderer   (visage, slots : peau/yeux/bouche/sourcils)
//       │
//       ├── [HairRoot]                 ← parent de toutes les coiffures
//       │   ├── Hair_Short  (désactivé par défaut)
//       │   ├── Hair_Long   (désactivé par défaut)
//       │   ├── Hair_Afro   (désactivé par défaut)
//       │   └── …
//       │
//       ├── [FaceDetailsRoot]          ← parent des meshes de visage swappables
//       │   ├── Eyes_Default  (actif par défaut)
//       │   ├── Eyes_Almond
//       │   ├── Nose_Default  (actif par défaut)
//       │   ├── Nose_Large
//       │   ├── Beard_None    (actif par défaut — mesh vide ou désactivé)
//       │   ├── Beard_Short
//       │   ├── Beard_Full
//       │   ├── Brows_Default (actif par défaut)
//       │   └── …
//       │
//       ├── [HatRoot]                  ← parent de tous les chapeaux
//       │   ├── Hat_Cap     (désactivé par défaut)
//       │   ├── Hat_Beanie  (désactivé par défaut)
//       │   └── …
//       │
//       └── [AccessoryRoot]            ← parent de tous les accessoires
//           ├── Glasses_Round  (désactivé par défaut)
//           ├── Earring_Hoop   (désactivé par défaut)
//           └── …
//
// SLOTS MATÉRIAUX sur Face_SkinnedMeshRenderer (à adapter selon ton mesh) :
//   0 = Peau
//   1 = Yeux
//   2 = Bouche / Lèvres
//   3 = Sourcils
// ============================================================
using UnityEngine;

public class CharacterPreviewController : MonoBehaviour
{
    // ================================================================
    // RÉFÉRENCES MANNEQUIN
    // ================================================================

    [Header("Renderers")]
    [Tooltip("SkinnedMeshRenderer du corps (tenue)")]
    [SerializeField] private SkinnedMeshRenderer _bodyRenderer;
    [Tooltip("SkinnedMeshRenderer du visage")]
    [SerializeField] private SkinnedMeshRenderer _faceRenderer;

    [Header("Roots des enfants swappables")]
    [SerializeField] private Transform _hairRoot;
    [SerializeField] private Transform _faceDetailsRoot;
    [SerializeField] private Transform _hatRoot;
    [SerializeField] private Transform _accessoryRoot;

    [Header("Slots matériaux sur le FaceRenderer")]
    [SerializeField] private int _slotPeau     = 0;
    [SerializeField] private int _slotYeux     = 1;
    [SerializeField] private int _slotBouche   = 2;
    [SerializeField] private int _slotSourceils = 3;

    [Header("Données")]
    [SerializeField] private CharacterCustomizationDef _def;

    // ================================================================
    // ROTATION ORBIT
    // ================================================================

    [Header("Rotation")]
    [SerializeField] private float _vitesseRotation   = 200f;
    [SerializeField] private float _smoothing         = 10f;
    [SerializeField] private float _vitesseAutoRotate = 15f;
    [SerializeField] private bool  _autoRotate        = true;

    private float   _rotationCible    = 0f;
    private float   _rotationActuelle = 0f;
    private bool    _isDragging       = false;
    private Vector3 _lastMousePos;

    // ================================================================
    // LIFECYCLE
    // ================================================================

    private void Start()
    {
        // Applique les données sauvegardées dès l'entrée dans la scène
        if (GameManager.Instance != null)
            AppliquerTout(GameManager.Instance.Personnalisation);
    }

    private void Update()
    {
        GererOrbit();
    }

    // ================================================================
    // ROTATION ORBIT (drag souris gauche)
    // ================================================================

    private void GererOrbit()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isDragging  = true;
            _autoRotate  = false;
            _lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
            _isDragging = false;

        if (_isDragging)
        {
            float delta    = Input.mousePosition.x - _lastMousePos.x;
            _rotationCible += delta * _vitesseRotation * Time.deltaTime;
            _lastMousePos   = Input.mousePosition;
        }
        else if (_autoRotate)
        {
            _rotationCible += _vitesseAutoRotate * Time.deltaTime;
        }

        _rotationActuelle = Mathf.LerpAngle(
            _rotationActuelle, _rotationCible, Time.deltaTime * _smoothing);

        transform.rotation = Quaternion.Euler(0f, _rotationActuelle, 0f);
    }

    // ================================================================
    // APPLICATION GLOBALE
    // ================================================================

    /// <summary>Applique toute une CharacterCustomizationData d'un coup.</summary>
    public void AppliquerTout(CharacterCustomizationData data)
    {
        if (data == null || _def == null) return;

        AppliquerYeux(data.IndexYeux);
        AppliquerNez(data.IndexNez);
        AppliquerBouche(data.IndexBouche);
        AppliquerSourceils(data.IndexSourceils);
        AppliquerBarbe(data.IndexBarbe);
        AppliquerCoiffure(data.IndexCoiffure);
        AppliquerCouleurCheveux(data.IndexCouleurCheveux);
        AppliquerTenue(data.IndexTenue);
        AppliquerChapeau(data.IndexChapeau);
        AppliquerAccessoire(data.IndexAccessoire);
        AppliquerCouleurPeau(data.IndexCouleurPeau);
    }

    // ================================================================
    // VISAGE — swap par GameObject enfant dans _faceDetailsRoot
    // ================================================================

    public void AppliquerYeux(int index)
    {
        if (_def == null || _def.Yeux == null || index >= _def.Yeux.Length) return;
        SwapEnfantDansRoot(_faceDetailsRoot, _def.Yeux[index].NomObjetEnfant, prefixe: "Eyes_");
        SwapMateriau(_faceRenderer, _slotYeux, _def.Yeux[index].Materiau);
    }

    public void AppliquerNez(int index)
    {
        if (_def == null || _def.Nez == null || index >= _def.Nez.Length) return;
        SwapEnfantDansRoot(_faceDetailsRoot, _def.Nez[index].NomObjetEnfant, prefixe: "Nose_");
    }

    public void AppliquerBouche(int index)
    {
        if (_def == null || _def.Bouches == null || index >= _def.Bouches.Length) return;
        SwapEnfantDansRoot(_faceDetailsRoot, _def.Bouches[index].NomObjetEnfant, prefixe: "Mouth_");
        SwapMateriau(_faceRenderer, _slotBouche, _def.Bouches[index].Materiau);
    }

    public void AppliquerSourceils(int index)
    {
        if (_def == null || _def.Sourcils == null || index >= _def.Sourcils.Length) return;
        SwapEnfantDansRoot(_faceDetailsRoot, _def.Sourcils[index].NomObjetEnfant, prefixe: "Brows_");
        SwapMateriau(_faceRenderer, _slotSourceils, _def.Sourcils[index].Materiau);
    }

    public void AppliquerBarbe(int index)
    {
        if (_def == null || _def.Barbes == null || index >= _def.Barbes.Length) return;
        SwapEnfantDansRoot(_faceDetailsRoot, _def.Barbes[index].NomObjetEnfant, prefixe: "Beard_");
    }

    // ================================================================
    // CHEVEUX
    // ================================================================

    public void AppliquerCoiffure(int index)
    {
        if (_def == null || _def.Coiffures == null || index >= _def.Coiffures.Length) return;

        string nomCible = _def.Coiffures[index].NomObjetEnfant;
        DesactiverTousEnfants(_hairRoot);

        if (!string.IsNullOrEmpty(nomCible))
        {
            Transform cible = _hairRoot?.Find(nomCible);
            if (cible != null) cible.gameObject.SetActive(true);
            else Debug.LogWarning($"[Preview] Coiffure introuvable : '{nomCible}'");
        }
        // Si NomObjetEnfant vide = chauve, rien à activer.
    }

    public void AppliquerCouleurCheveux(int index)
    {
        if (_def == null || _def.CouleursCheveux == null) return;
        if (index < 0 || index >= _def.CouleursCheveux.Length) return;

        Color couleur = _def.CouleursCheveux[index];

        // Applique sur tous les enfants actifs de _hairRoot
        if (_hairRoot != null)
        {
            foreach (Transform enfant in _hairRoot)
            {
                if (!enfant.gameObject.activeSelf) continue;
                var r = enfant.GetComponent<Renderer>();
                if (r != null)
                {
                    // Crée une instance de mat pour ne pas modifier l'asset
                    r.material.color = couleur;
                }
            }
        }
    }

    // ================================================================
    // TENUE
    // ================================================================

    public void AppliquerTenue(int index)
    {
        if (_def == null || _def.Tenues == null || index >= _def.Tenues.Length) return;
        if (_bodyRenderer == null) return;

        OutfitOption tenue = _def.Tenues[index];

        // Matériaux sur le corps
        if (tenue.Materiaux != null && tenue.Materiaux.Length > 0)
        {
            Material[] mats = _bodyRenderer.materials;
            for (int i = 0; i < tenue.Materiaux.Length && i < mats.Length; i++)
                if (tenue.Materiaux[i] != null) mats[i] = tenue.Materiaux[i];
            _bodyRenderer.materials = mats;
        }

        // Accessoire de tenue (capuche, ceinture…)
        // Note : on ne touche pas à _hatRoot ni _accessoryRoot ici.
    }

    // ================================================================
    // CHAPEAU
    // Si le chapeau CacheCheveux = true, on désactive _hairRoot.
    // ================================================================

    public void AppliquerChapeau(int index)
    {
        if (_def == null || _def.Chapeaux == null || index >= _def.Chapeaux.Length) return;

        HatOption chapeau = _def.Chapeaux[index];
        DesactiverTousEnfants(_hatRoot);

        if (!string.IsNullOrEmpty(chapeau.NomObjetEnfant))
        {
            Transform cible = _hatRoot?.Find(chapeau.NomObjetEnfant);
            if (cible != null) cible.gameObject.SetActive(true);
        }

        // Cache ou affiche les cheveux selon le chapeau
        if (_hairRoot != null)
            _hairRoot.gameObject.SetActive(!chapeau.CacheCheveux);
    }

    // ================================================================
    // ACCESSOIRES
    // ================================================================

    public void AppliquerAccessoire(int index)
    {
        if (_def == null || _def.Accessoires == null || index >= _def.Accessoires.Length) return;

        AccessoryOption acc = _def.Accessoires[index];
        DesactiverTousEnfants(_accessoryRoot);

        if (!string.IsNullOrEmpty(acc.NomObjetEnfant))
        {
            Transform cible = _accessoryRoot?.Find(acc.NomObjetEnfant);
            if (cible != null) cible.gameObject.SetActive(true);
        }
    }

    // ================================================================
    // PEAU — slot 0 sur Body + Face
    // ================================================================

    public void AppliquerCouleurPeau(int index)
    {
        if (_def == null || _def.CouleursPeau == null) return;
        if (index < 0 || index >= _def.CouleursPeau.Length) return;

        Color couleur = _def.CouleursPeau[index];

        if (_bodyRenderer != null)
        {
            // Slot 0 = peau (premier matériau du corps)
            Material[] mats = _bodyRenderer.materials;
            if (mats.Length > _slotPeau)
            {
                mats[_slotPeau].color = couleur;
                _bodyRenderer.materials = mats;
            }
        }

        if (_faceRenderer != null)
        {
            Material[] mats = _faceRenderer.materials;
            if (mats.Length > _slotPeau)
            {
                mats[_slotPeau].color = couleur;
                _faceRenderer.materials = mats;
            }
        }
    }

    // ================================================================
    // HELPERS PRIVÉS
    // ================================================================

    /// <summary>
    /// Désactive tous les enfants de root dont le nom commence par [prefixe],
    /// puis active uniquement [nomCible].
    /// Si nomCible est vide, tout reste désactivé (option "Aucun").
    /// </summary>
    private void SwapEnfantDansRoot(Transform root, string nomCible, string prefixe)
    {
        if (root == null) return;

        // Désactive uniquement les enfants concernés par ce prefixe
        foreach (Transform enfant in root)
        {
            if (enfant.name.StartsWith(prefixe))
                enfant.gameObject.SetActive(false);
        }

        if (!string.IsNullOrEmpty(nomCible))
        {
            Transform cible = root.Find(nomCible);
            if (cible != null)
                cible.gameObject.SetActive(true);
            else
                Debug.LogWarning($"[Preview] Objet enfant introuvable : '{nomCible}' dans '{root.name}'");
        }
    }

    /// <summary>Désactive tous les enfants directs d'un root.</summary>
    private void DesactiverTousEnfants(Transform root)
    {
        if (root == null) return;
        foreach (Transform enfant in root)
            enfant.gameObject.SetActive(false);
    }

    /// <summary>Swap un matériau sur un slot donné d'un SkinnedMeshRenderer.</summary>
    private void SwapMateriau(SkinnedMeshRenderer smr, int slot, Material mat)
    {
        if (smr == null || mat == null) return;
        Material[] mats = smr.materials;
        if (slot >= 0 && slot < mats.Length)
        {
            mats[slot] = mat;
            smr.materials = mats;
        }
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public CharacterCustomizationDef Def => _def;
}
