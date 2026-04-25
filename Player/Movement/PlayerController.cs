// ============================================================
// PlayerController.cs — Bailiff & Co
// Déplacements, sprint, accroupissement, allongement, saut.
// ============================================================
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerNoiseEmitter))]
public class PlayerController : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float _vitesseNormale    = 4f;
    [SerializeField] private float _vitesseSprint     = 7f;
    [SerializeField] private float _vitesseAccroupi   = 2f;
    [SerializeField] private float _vitesseAllonge    = 1f;
    [SerializeField] private float _gravite           = -9.81f;

    [Header("Saut")]
    [SerializeField] private float _forceVSaut        = 4f;
    [SerializeField] private float _cooldownSaut      = 0.5f;

    [Header("Caméra")]
    [SerializeField] private Transform _camera;
    [SerializeField] private float     _sensibiliteSouris     = 2f;
    [SerializeField] private float     _clampVertical         = 60f;
    [SerializeField] private float     _hauteurCameraNormale  = 1.8f;
    [SerializeField] private float     _hauteurCameraAccroupi = 1.25f;
    [SerializeField] private float     _hauteurCameraAllonge  = 0.2f;
    [SerializeField] private float     _vitesseCameraLerp     = 8f;

    [Header("Hauteur CharacterController")]
    [SerializeField] private float _hauteurNormale           = 1.8f;
    [SerializeField] private float _hauteurAccroupi          = 1.2f;
    [SerializeField] private float _hauteurAllonge           = 0.15f;
    [SerializeField] private float _vitesseChangementHauteur = 8f;

    private CharacterController _cc;
    private PlayerNoiseEmitter  _noise;
    private PlayerInteractor    _interactor;
    private PauseMenu           _pauseMenu;

    private Vector3 _velociteXZ    = Vector3.zero;
    private float   _velociteY     = 0f;

    private float   _rotationX     = 0f;
    private bool    _estAccroupi   = false;
    private bool    _estAllonge    = false;
    private bool    _estAuSol      = false;
    private float   _dernierSaut   = -999f;
    private string  _tagSol        = "";

    private const float COYOTE_TIME = 0.15f;
    private float _dernierTempsAuSol = 0f;

    private void Awake()
    {
        _cc         = GetComponent<CharacterController>();
        _noise      = GetComponent<PlayerNoiseEmitter>();
        _interactor = GetComponent<PlayerInteractor>();
        _pauseMenu  = FindObjectOfType<PauseMenu>(includeInactive: true);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Bloque tout input si le menu pause est ouvert
        if (_pauseMenu != null && _pauseMenu.EstOuvert) return;

        DetecterSol();
        GererGravite();
        GererCamera();
        GererPosture();
        GererMouvement();
        GererSaut();
        AdapterHauteur();
        AdapterCamera();
    }

    // ================================================================
    // VÉRIFICATION ESPACE LIBRE AU-DESSUS
    // Fait un SphereCast vers le haut pour savoir si on peut
    // se relever jusqu'à la hauteur cible.
    // ================================================================

    private bool EspaceLibrePour(float hauteurCible)
    {
        // Le bas du CC est toujours à transform.position (center.y = height/2)
        // On part du centre actuel et on teste si la hauteur cible rentre
        float hauteurActuelle = _cc.height;
        float difference      = hauteurCible - hauteurActuelle;

        if (difference <= 0f) return true; // on se baisse, toujours autorisé

        // Origine du cast : sommet actuel du CC
        Vector3 origine = transform.position + Vector3.up * hauteurActuelle;
        float   radius  = _cc.radius * 0.9f;

        // On teste si la différence de hauteur est libre
        bool bloque = Physics.SphereCast(
            origine, radius, Vector3.up,
            out _, difference,
            Physics.AllLayers, QueryTriggerInteraction.Ignore);

        return !bloque;
    }

    // ================================================================
    // DÉTECTION SOL
    // ================================================================

    private void DetecterSol()
    {
        float basCC = _cc.center.y - _cc.height * 0.5f;
        Vector3 bas = transform.position + Vector3.up * (basCC + 0.05f);
        float dist  = 0.35f;

        bool c = Physics.Raycast(bas, Vector3.down, out RaycastHit hit, dist,
                     Physics.AllLayers, QueryTriggerInteraction.Ignore);
        bool a = Physics.Raycast(bas + transform.forward  * 0.2f, Vector3.down,
                     dist, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        bool b = Physics.Raycast(bas - transform.forward  * 0.2f, Vector3.down,
                     dist, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        _estAuSol = _cc.isGrounded || c || a || b;

        if (_estAuSol)
        {
            _dernierTempsAuSol = Time.time;
            if (hit.collider != null) _tagSol = hit.collider.tag;
        }
    }

    // ================================================================
    // CAMÉRA
    // ================================================================

    private void GererCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensibiliteSouris;
        float mouseY = Input.GetAxis("Mouse Y") * _sensibiliteSouris;

        _rotationX -= mouseY;
        _rotationX  = Mathf.Clamp(_rotationX, -_clampVertical, _clampVertical);

        if (_camera != null)
            _camera.localRotation = Quaternion.Euler(_rotationX, 0, 0);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void AdapterCamera()
    {
        if (_camera == null) return;

        float cibleY = _estAllonge  ? _hauteurCameraAllonge
                     : _estAccroupi ? _hauteurCameraAccroupi
                     :                _hauteurCameraNormale;

        Vector3 pos = _camera.localPosition;
        pos.y = Mathf.Lerp(pos.y, cibleY, Time.deltaTime * _vitesseCameraLerp);
        _camera.localPosition = pos;
    }

    // ================================================================
    // POSTURE — vérifie l'espace libre avant de se relever
    // ================================================================

    private void GererPosture()
    {
        // Ctrl : toggle accroupi / debout
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (_estAllonge)
            {
                // Allongé → accroupi : vérifie hauteur accroupi
                if (EspaceLibrePour(_hauteurAccroupi))
                {
                    _estAllonge  = false;
                    _estAccroupi = true;
                }
            }
            else if (_estAccroupi)
            {
                // Accroupi → debout : vérifie hauteur normale
                if (EspaceLibrePour(_hauteurNormale))
                {
                    _estAccroupi = false;
                }
            }
            else
            {
                // Debout → accroupi : toujours autorisé (on se baisse)
                _estAccroupi = true;
            }
        }

        // X : toggle allongé / debout
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (_estAllonge)
            {
                // Allongé → debout : vérifie hauteur normale
                if (EspaceLibrePour(_hauteurNormale))
                {
                    _estAllonge  = false;
                    _estAccroupi = false;
                }
                // Sinon essaie au moins de passer accroupi
                else if (EspaceLibrePour(_hauteurAccroupi))
                {
                    _estAllonge  = false;
                    _estAccroupi = true;
                }
            }
            else
            {
                // Debout ou accroupi → allongé : toujours autorisé
                _estAccroupi = false;
                _estAllonge  = true;
            }
        }
    }

    // ================================================================
    // MOUVEMENT
    // ================================================================

    private void GererMouvement()
    {
        var hubUI = FindObjectOfType<HubUI>();
        if (hubUI != null && hubUI.UnPanneauEstOuvert) return;
        
        if (_estAuSol)
        {
            bool sprint = Input.GetKey(KeyCode.LeftShift)
                          && !_estAccroupi && !_estAllonge;

            float vitesseBase = _estAllonge  ? _vitesseAllonge
                              : _estAccroupi ? _vitesseAccroupi
                              : sprint       ? _vitesseSprint
                              :                _vitesseNormale;

            // Réduit la vitesse si on pousse un meuble
            float multiMeuble = _interactor != null ? _interactor.MultiplicateurVitesseMeuble : 1f;
            float vitesse = vitesseBase * multiMeuble;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 dir = Vector3.ClampMagnitude(
                transform.right * h + transform.forward * v, 1f);

            _velociteXZ = dir * vitesse;
            _cc.Move(_velociteXZ * Time.deltaTime);

            if (dir.magnitude > 0.1f)
                EmettreBruitDeplacement(sprint);
        }
        else
        {
            _cc.Move(_velociteXZ * Time.deltaTime);
        }
    }

    private void EmettreBruitDeplacement(bool sprint)
    {
        if (_estAllonge || _estAccroupi)
        {
            _noise.EmettreBruit(NiveauBruit.Silencieux, 0f);
            return;
        }

        NiveauBruit niveau = sprint ? NiveauBruit.Fort : NiveauBruit.Leger;
        float portee = sprint
            ? _tagSol switch { "Carrelage" => 14f, "Parquet" => 12f, "Moquette" => 6f, _ => 10f }
            : _tagSol switch { "Carrelage" => 7f,  "Parquet" => 5f,  "Moquette" => 2f, _ => 5f  };

        _noise.EmettreBruit(niveau, portee);
    }

    // ================================================================
    // SAUT
    // ================================================================

    private void GererSaut()
    {
        bool cooldownOk  = (Time.time - _dernierSaut) >= _cooldownSaut;
        bool coyoteOk    = (Time.time - _dernierTempsAuSol) < COYOTE_TIME;
        bool modDeblocage = cooldownOk && !_estAuSol
                            && (Time.time - _dernierTempsAuSol) > 2f;

        bool peutSauter = cooldownOk && (coyoteOk || _estAuSol);

        if ((peutSauter || modDeblocage)
            && !_estAccroupi
            && !_estAllonge
            && _velociteY <= 0.1f
            && Input.GetKeyDown(KeyCode.Space))
        {
            _velociteY         = _forceVSaut;
            _dernierSaut       = Time.time;
            _dernierTempsAuSol = -999f;
            _noise.EmettreBruit(NiveauBruit.Leger, 3f);
        }
    }

    // ================================================================
    // GRAVITÉ
    // ================================================================

    private void GererGravite()
    {
        if (_estAuSol && _velociteY < 0)
            _velociteY = -2f;

        _velociteY += _gravite * Time.deltaTime;
        _cc.Move(Vector3.up * _velociteY * Time.deltaTime);
    }

    // ================================================================
    // HAUTEUR CHARACTERCONTROLLER
    // ================================================================

    private void AdapterHauteur()
    {
        float cible = _estAllonge  ? _hauteurAllonge
                    : _estAccroupi ? _hauteurAccroupi
                    :                _hauteurNormale;

        _cc.height = Mathf.Lerp(_cc.height, cible,
                                 Time.deltaTime * _vitesseChangementHauteur);
        _cc.center = new Vector3(0, _cc.height / 2f, 0);
    }

    // ================================================================
    // PROPRIÉTÉS
    // ================================================================

    public bool EstAccroupi    => _estAccroupi;
    public bool EstAllonge     => _estAllonge;
    public bool EstAuSol       => _estAuSol;
    public bool EstEnMouvement =>
        Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
        Mathf.Abs(Input.GetAxisRaw("Vertical"))   > 0.1f;
}
