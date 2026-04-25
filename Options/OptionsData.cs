// ============================================================
// OptionsData.cs — Bailiff & Co
// Données persistantes de toutes les options du jeu.
// Sérialisée via JsonUtility → PlayerPrefs.
// ============================================================
using UnityEngine;

[System.Serializable]
public class OptionsData
{
    // ── VIDÉO ────────────────────────────────────────────────
    public int   IndexResolution    = -1;
    public int   ModeEcran          = 0;    // 0=Fullscreen, 1=Windowed, 2=Borderless
    public int   QualiteGlobale     = 2;    // 0=Low, 1=Medium, 2=High, 3=Ultra
    public bool  VSync              = true;
    public int   LimiteFPS          = 0;    // 0=illimité, 1=30, 2=60, 3=120, 4=144, 5=240

    // ── AUDIO ────────────────────────────────────────────────
    public float VolumeMaster       = 1f;
    public float VolumeMusique      = 0.7f;
    public float VolumeSFX          = 1f;
    public float VolumeAmbiance     = 0.8f;

    // ── SOURIS ───────────────────────────────────────────────
    public float SensibiliteSouris  = 2f;
    public bool  InverserY          = false;

    // ── TOUCHES DÉPLACEMENT ──────────────────────────────────
    public int ToucheAvancer        = (int)KeyCode.Z;
    public int ToucheReculer        = (int)KeyCode.S;
    public int ToucheGauche         = (int)KeyCode.Q;
    public int ToucheDroite         = (int)KeyCode.D;

    // ── TOUCHES ACTIONS ──────────────────────────────────────
    public int ToucheInteragir      = (int)KeyCode.E;
    public int ToucheSprint         = (int)KeyCode.LeftShift;
    public int ToucheAccroupi       = (int)KeyCode.LeftControl;
    public int ToucheAllonge        = (int)KeyCode.X;
    public int ToucheSaut           = (int)KeyCode.Space;
    public int ToucheInventaire     = (int)KeyCode.Tab;
    public int TouchePause          = (int)KeyCode.Escape;

    // ================================================================
    // PERSISTANCE JSON
    // ================================================================

    private const string PREFS_KEY = "BailiffCo_Options";

    public static OptionsData Charger()
    {
        if (PlayerPrefs.HasKey(PREFS_KEY))
        {
            try
            {
                string json = PlayerPrefs.GetString(PREFS_KEY);
                var data = new OptionsData();
                JsonUtility.FromJsonOverwrite(json, data);
                return data;
            }
            catch
            {
                Debug.LogWarning("[OptionsData] Données corrompues, reset aux défauts.");
            }
        }
        return new OptionsData();
    }

    public void Sauvegarder()
    {
        string json = JsonUtility.ToJson(this, prettyPrint: true);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[OptionsData] Sauvegardé :\n{json}");
    }

    public void Reset()
    {
        var d = new OptionsData();
        IndexResolution   = d.IndexResolution;
        ModeEcran         = d.ModeEcran;
        QualiteGlobale    = d.QualiteGlobale;
        VSync             = d.VSync;
        LimiteFPS         = d.LimiteFPS;
        VolumeMaster      = d.VolumeMaster;
        VolumeMusique     = d.VolumeMusique;
        VolumeSFX         = d.VolumeSFX;
        VolumeAmbiance    = d.VolumeAmbiance;
        SensibiliteSouris = d.SensibiliteSouris;
        InverserY         = d.InverserY;
        ToucheAvancer     = d.ToucheAvancer;
        ToucheReculer     = d.ToucheReculer;
        ToucheGauche      = d.ToucheGauche;
        ToucheDroite      = d.ToucheDroite;
        ToucheInteragir   = d.ToucheInteragir;
        ToucheSprint      = d.ToucheSprint;
        ToucheAccroupi    = d.ToucheAccroupi;
        ToucheAllonge     = d.ToucheAllonge;
        ToucheSaut        = d.ToucheSaut;
        ToucheInventaire  = d.ToucheInventaire;
        TouchePause       = d.TouchePause;
    }

    // ================================================================
    // ACCESSEURS KEYCODE
    // ================================================================

    public KeyCode GetTouche(ActionJeu action) => action switch
    {
        ActionJeu.Avancer    => (KeyCode)ToucheAvancer,
        ActionJeu.Reculer    => (KeyCode)ToucheReculer,
        ActionJeu.Gauche     => (KeyCode)ToucheGauche,
        ActionJeu.Droite     => (KeyCode)ToucheDroite,
        ActionJeu.Interagir  => (KeyCode)ToucheInteragir,
        ActionJeu.Sprint     => (KeyCode)ToucheSprint,
        ActionJeu.Accroupi   => (KeyCode)ToucheAccroupi,
        ActionJeu.Allonge    => (KeyCode)ToucheAllonge,
        ActionJeu.Saut       => (KeyCode)ToucheSaut,
        ActionJeu.Inventaire => (KeyCode)ToucheInventaire,
        ActionJeu.Pause      => (KeyCode)TouchePause,
        _                    => KeyCode.None
    };

    public void SetTouche(ActionJeu action, KeyCode key)
    {
        switch (action)
        {
            case ActionJeu.Avancer:    ToucheAvancer    = (int)key; break;
            case ActionJeu.Reculer:    ToucheReculer    = (int)key; break;
            case ActionJeu.Gauche:     ToucheGauche     = (int)key; break;
            case ActionJeu.Droite:     ToucheDroite     = (int)key; break;
            case ActionJeu.Interagir:  ToucheInteragir  = (int)key; break;
            case ActionJeu.Sprint:     ToucheSprint     = (int)key; break;
            case ActionJeu.Accroupi:   ToucheAccroupi   = (int)key; break;
            case ActionJeu.Allonge:    ToucheAllonge    = (int)key; break;
            case ActionJeu.Saut:       ToucheSaut       = (int)key; break;
            case ActionJeu.Inventaire: ToucheInventaire = (int)key; break;
            case ActionJeu.Pause:      TouchePause      = (int)key; break;
        }
    }
}

// ── Enum complet des actions rebindables ─────────────────────
public enum ActionJeu
{
    // Déplacements
    Avancer,
    Reculer,
    Gauche,
    Droite,
    // Actions
    Interagir,
    Sprint,
    Accroupi,
    Allonge,
    Saut,
    Inventaire,
    Pause
}
