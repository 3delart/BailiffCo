// ============================================================
// GUIDE_SceneCharacterCustomization.cs — Bailiff & Co
// Guide de setup Unity pour la scène CharacterCustomization.
// NE PAS importer dans Unity — c'est un guide de référence.
// ============================================================

/*
════════════════════════════════════════════════════════════════
ÉTAPE 1 — CRÉER LA SCÈNE
════════════════════════════════════════════════════════════════

1. File → New Scene → Basic (Built-in)
2. Sauvegarder : Assets/Scenes/CharacterCustomization.unity
3. Ajouter dans Build Settings (File → Build Settings → Add Open Scenes)


════════════════════════════════════════════════════════════════
ÉTAPE 2 — CAMÉRA DE PREVIEW
════════════════════════════════════════════════════════════════

Créer un GameObject "PreviewCamera" :
  ├── Camera (Depth = 1, Clear Flags = Solid Color, fond sombre)
  ├── Positionner face au mannequin (ex: Position 0, 1, -3 / Rotation 0, 0, 0)
  └── Output Texture = nouvelle RenderTexture 512x512 (créer dans Project)

Dans le Canvas → RawImage "ZonePreview" :
  └── Texture = la RenderTexture créée ci-dessus


════════════════════════════════════════════════════════════════
ÉTAPE 3 — MANNEQUIN
════════════════════════════════════════════════════════════════

Hiérarchie recommandée :

  PreviewRoot  ← CharacterPreviewController.cs ici
               Position : 0, 0, 0
  └── Mannequin  (ton rig importé, SkinnedMeshRenderer sur Body et Face)
      │
      ├── Body_SMR  ← SkinnedMeshRenderer corps + tenue
      │              Matériaux : [0]=Peau, [1]=Tenue...
      │
      ├── Face_SMR  ← SkinnedMeshRenderer visage
      │              Matériaux : [0]=Peau, [1]=Yeux, [2]=Bouche, [3]=Sourcils
      │
      ├── HairRoot  ← Transform vide, parent de toutes les coiffures
      │   ├── Hair_Short     (désactivé par défaut)
      │   ├── Hair_Long      (désactivé par défaut)
      │   ├── Hair_Afro      (désactivé par défaut)
      │   └── Hair_Curly     (désactivé par défaut)
      │
      ├── FaceDetailsRoot  ← parent des meshes de visage swappables
      │   ├── Eyes_Default   (ACTIF par défaut)
      │   ├── Eyes_Almond    (désactivé)
      │   ├── Eyes_Round     (désactivé)
      │   ├── Nose_Default   (ACTIF par défaut)
      │   ├── Nose_Large     (désactivé)
      │   ├── Nose_Small     (désactivé)
      │   ├── Mouth_Default  (ACTIF par défaut)
      │   ├── Mouth_Wide     (désactivé)
      │   ├── Brows_Default  (ACTIF par défaut)
      │   ├── Brows_Thin     (désactivé)
      │   ├── Brows_Thick    (désactivé)
      │   ├── Beard_None     (ACTIF par défaut — mesh vide ou invisible)
      │   ├── Beard_Stubble  (désactivé)
      │   ├── Beard_Short    (désactivé)
      │   └── Beard_Full     (désactivé)
      │
      ├── HatRoot  ← parent de tous les chapeaux
      │   ├── Hat_Cap        (désactivé par défaut)
      │   ├── Hat_Beanie     (désactivé)
      │   └── Hat_Fedora     (désactivé)
      │
      └── AccessoryRoot  ← parent de tous les accessoires
          ├── Glasses_Round  (désactivé par défaut)
          ├── Glasses_Square (désactivé)
          └── Earring_Hoop   (désactivé)


════════════════════════════════════════════════════════════════
ÉTAPE 4 — SCRIPTABLE OBJECT CharacterCustomizationDef
════════════════════════════════════════════════════════════════

1. Clic droit dans Project → Create → BailiffCo → CharacterCustomizationDef
2. Nommer l'asset "CharacterCustomizationDef_Default"
3. Remplir chaque liste :

   Yeux :
     [0] Nom="Défaut"  NomObjetEnfant="Eyes_Default"
     [1] Nom="Amande"  NomObjetEnfant="Eyes_Almond"
     [2] Nom="Rond"    NomObjetEnfant="Eyes_Round"

   Nez :
     [0] Nom="Défaut"  NomObjetEnfant="Nose_Default"
     [1] Nom="Large"   NomObjetEnfant="Nose_Large"
     [2] Nom="Petit"   NomObjetEnfant="Nose_Small"

   Bouches :
     [0] Nom="Défaut"  NomObjetEnfant="Mouth_Default"
     [1] Nom="Large"   NomObjetEnfant="Mouth_Wide"

   Sourcils :
     [0] Nom="Défaut"  NomObjetEnfant="Brows_Default"
     [1] Nom="Fins"    NomObjetEnfant="Brows_Thin"
     [2] Nom="Épais"   NomObjetEnfant="Brows_Thick"

   Barbes :
     [0] Nom="Aucune"   NomObjetEnfant=""           ← vide = aucune
     [1] Nom="Barbe"    NomObjetEnfant="Beard_Short"
     [2] Nom="Complète" NomObjetEnfant="Beard_Full"

   Coiffures :
     [0] Nom="Chauve"   NomObjetEnfant=""            ← vide = chauve
     [1] Nom="Court"    NomObjetEnfant="Hair_Short"
     [2] Nom="Long"     NomObjetEnfant="Hair_Long"

   CouleursCheveux : (ajouter des Color)
     Noir, Brun, Blond, Roux, Gris, Blanc, Bleu, Rouge…

   Tenues :
     [0] Nom="Tenue Huissier"  Materiaux=[mat_costume]
     [1] Nom="Casual"          Materiaux=[mat_casual]

   Chapeaux :
     [0] Nom="Aucun"   NomObjetEnfant=""  CacheCheveux=false
     [1] Nom="Casquette" NomObjetEnfant="Hat_Cap"  CacheCheveux=false
     [2] Nom="Bonnet"    NomObjetEnfant="Hat_Beanie" CacheCheveux=true

   Accessoires :
     [0] Nom="Aucun"    NomObjetEnfant=""
     [1] Nom="Lunettes" NomObjetEnfant="Glasses_Round"

   CouleursPeau : (ajouter des Color)
     Clair, Beige, Brun clair, Brun, Brun foncé, Ébène…


════════════════════════════════════════════════════════════════
ÉTAPE 5 — CANVAS UI
════════════════════════════════════════════════════════════════

Canvas (Screen Space Overlay)
├── EventSystem
├── FondArrierePlan (Image noire semi-transparente full-screen)
│
├── ZonePreview (RawImage, côté gauche ou centré)
│   └── Texture = RenderTexture de la caméra preview
│
├── BarreOngletsPrincipaux (HorizontalLayoutGroup)
│   ├── BtnOngletVisage    (Button + Image)    → texte "👁 Visage"
│   ├── BtnOngletCheveux   (Button + Image)    → texte "💇 Cheveux"
│   ├── BtnOngletTenue     (Button + Image)    → texte "👔 Tenue"
│   └── BtnOngletPeau      (Button + Image)    → texte "🎨 Peau"
│
├── PanneauVisage
│   ├── BarreSousOnglets (HorizontalLayoutGroup)
│   │   ├── BtnYeux / BtnNez / BtnBouche / BtnSourceils / BtnBarbe
│   └── [SélecteurOption] ← voir ci-dessous
│
├── PanneauCheveux
│   ├── BarreSousOnglets
│   │   ├── BtnCoiffure / BtnCouleurCheveux
│   ├── [SélecteurOption]
│   └── GridCouleursCheveux (GridLayoutGroup, pastilles colorées)
│
├── PanneauTenue
│   ├── BarreSousOnglets
│   │   ├── BtnTenue / BtnChapeau / BtnAccessoire
│   └── [SélecteurOption]
│
├── PanneauPeau
│   └── GridCouleursPeau (GridLayoutGroup, pastilles colorées)
│
├── [SélecteurOption] — structure partagée
│   ├── BtnGauche  (Button "◄")
│   ├── InfoOption
│   │   ├── IconeOption  (Image, 64x64)
│   │   ├── TexteNomOption (TMP)
│   │   └── TexteIndex (TMP "1 / 5")
│   └── BtnDroite  (Button "►")
│
└── BasDePage (HorizontalLayoutGroup)
    ├── BtnAnnuler   (Button "Annuler")
    └── BtnConfirmer (Button "✓ Confirmer")


════════════════════════════════════════════════════════════════
ÉTAPE 6 — ASSIGNATION DES RÉFÉRENCES
════════════════════════════════════════════════════════════════

Sur CustomizationUI.cs (GameObject "UIManager") :
  _preview                → PreviewRoot (CharacterPreviewController)
  _btnOnglet*             → boutons onglets principaux
  _btn* (sous-onglets)    → boutons sous-onglets
  _btnGauche / Droite     → flèches
  _texteNomOption         → TMP nom
  _texteIndex             → TMP index
  _iconeOption            → Image icône
  _panneauFlechesOption   → parent [SélecteurOption]
  _gridCouleursPeau       → GridCouleursPeau
  _gridCouleursCheveux    → GridCouleursCheveux
  _prefabPastilleCouleur  → prefab bouton rond coloré
  _btnConfirmer/Annuler   → boutons bas de page

Sur CharacterPreviewController.cs (PreviewRoot) :
  _bodyRenderer           → Body_SMR
  _faceRenderer           → Face_SMR
  _hairRoot               → HairRoot
  _faceDetailsRoot        → FaceDetailsRoot
  _hatRoot                → HatRoot
  _accessoryRoot          → AccessoryRoot
  _def                    → CharacterCustomizationDef_Default (asset SO)

*/
