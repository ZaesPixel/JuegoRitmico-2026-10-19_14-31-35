using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Debe estar en un objeto que permanezca activo, fuera de PanelPausa.
[DefaultExecutionOrder(-500)]
public class MenuPausa : MonoBehaviour
{
    [Header("Interfaz (el creador del menu conecta estos campos)")]
    public GameObject panelPausa;
    public Slider barraMusica;
    public Slider barraEfectos;
    public Toggle pantallaCompleta;
    public Button botonReanudar;
    public Button botonSalir;
    public Text porcentajeMusica;
    public Text porcentajeEfectos;

    [Header("Arrastra aqui los AudioSource de tu juego")]
    public AudioSource musica;
    public AudioSource[] efectos = new AudioSource[0];

    [Header("Nombre exacto de la escena del menu principal")]
    public string escenaMenu = "MenuInicio";

    public static bool EstaEnPausa { get; private set; }

    private const string ClaveMusica = "MenuPausa.VolumenMusica";
    private const string ClaveEfectos = "MenuPausa.VolumenEfectos";
    private bool pausaPropia;
    private float tiempoAnterior;
    private bool audioAnterior;
    private bool cursorVisibleAnterior;
    private CursorLockMode cursorAnterior;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void RestablecerEstado()
    {
        EstaEnPausa = false;
    }

    private void Start()
    {
        if (panelPausa == null || panelPausa == gameObject ||
            transform.IsChildOf(panelPausa.transform))
        {
            Debug.LogError("MenuPausa: asigna un panel separado. El controlador debe quedar fuera del panel.", this);
            enabled = false;
            return;
        }

        panelPausa.SetActive(false);

        float volumenMusica = PlayerPrefs.GetFloat(ClaveMusica, musica != null ? musica.volume : 1f);
        float volumenEfectos = PlayerPrefs.GetFloat(ClaveEfectos, 1f);

        PrepararBarra(barraMusica, volumenMusica);
        PrepararBarra(barraEfectos, volumenEfectos);
        CambiarMusica(volumenMusica);
        CambiarEfectos(volumenEfectos);

        if (barraMusica != null) barraMusica.onValueChanged.AddListener(CambiarMusica);
        if (barraEfectos != null) barraEfectos.onValueChanged.AddListener(CambiarEfectos);
        if (pantallaCompleta != null)
        {
            pantallaCompleta.SetIsOnWithoutNotify(Screen.fullScreen);
            pantallaCompleta.onValueChanged.AddListener(CambiarPantallaCompleta);
        }
        if (botonReanudar != null) botonReanudar.onClick.AddListener(Reanudar);
        if (botonSalir != null) botonSalir.onClick.AddListener(SalirAlMenu);
    }

    private void Update()
    {
        bool presionoEsc = false;
#if ENABLE_LEGACY_INPUT_MANAGER || !ENABLE_INPUT_SYSTEM
        presionoEsc = Input.GetKeyDown(KeyCode.Escape);
#elif ENABLE_INPUT_SYSTEM
        presionoEsc = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#endif
        if (presionoEsc)
        {
            if (pausaPropia) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        if (pausaPropia || EstaEnPausa || panelPausa == null) return;

        tiempoAnterior = Time.timeScale;
        audioAnterior = AudioListener.pause;
        cursorAnterior = Cursor.lockState;
        cursorVisibleAnterior = Cursor.visible;
        pausaPropia = true;
        EstaEnPausa = true;

        Time.timeScale = 0f;
        // Congela tambien el reloj DSP para los juegos de ritmo.
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (pantallaCompleta != null)
            pantallaCompleta.SetIsOnWithoutNotify(Screen.fullScreen);
        panelPausa.SetActive(true);
    }

    public void Reanudar()
    {
        if (!pausaPropia) return;

        pausaPropia = false;
        EstaEnPausa = false;
        Time.timeScale = tiempoAnterior;
        AudioListener.pause = audioAnterior;
        Cursor.lockState = cursorAnterior;
        Cursor.visible = cursorVisibleAnterior;
        if (panelPausa != null) panelPausa.SetActive(false);
        PlayerPrefs.Save();
    }

    public void SalirAlMenu()
    {
        if (string.IsNullOrWhiteSpace(escenaMenu) ||
            !Application.CanStreamedLevelBeLoaded(escenaMenu))
        {
            Debug.LogError("MenuPausa: revisa Escena Menu y agrega esa escena a Build Profiles / Build Settings.", this);
            return;
        }

        Reanudar();
        // La escena inicial empieza con tiempo y audio normales.
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(escenaMenu);
    }

    public void CambiarMusica(float valor)
    {
        valor = Mathf.Clamp01(valor);
        if (musica != null) musica.volume = valor;
        if (porcentajeMusica != null) porcentajeMusica.text = Mathf.RoundToInt(valor * 100f) + "%";
        PlayerPrefs.SetFloat(ClaveMusica, valor);
    }

    public void CambiarEfectos(float valor)
    {
        valor = Mathf.Clamp01(valor);
        if (efectos != null)
        {
            foreach (AudioSource fuente in efectos)
            {
                if (fuente != null && fuente != musica) fuente.volume = valor;
            }
        }
        if (porcentajeEfectos != null) porcentajeEfectos.text = Mathf.RoundToInt(valor * 100f) + "%";
        PlayerPrefs.SetFloat(ClaveEfectos, valor);
    }

    public void CambiarPantallaCompleta(bool activar)
    {
        Screen.fullScreen = activar;
    }

    private static void PrepararBarra(Slider barra, float valor)
    {
        if (barra == null) return;
        barra.minValue = 0f;
        barra.maxValue = 1f;
        barra.wholeNumbers = false;
        barra.SetValueWithoutNotify(Mathf.Clamp01(valor));
    }

    private void OnDisable()
    {
        // Evita dejar el juego sin tiempo o sin audio al retirar el controlador.
        Reanudar();
    }

    private void OnDestroy()
    {
        if (barraMusica != null) barraMusica.onValueChanged.RemoveListener(CambiarMusica);
        if (barraEfectos != null) barraEfectos.onValueChanged.RemoveListener(CambiarEfectos);
        if (pantallaCompleta != null) pantallaCompleta.onValueChanged.RemoveListener(CambiarPantallaCompleta);
        if (botonReanudar != null) botonReanudar.onClick.RemoveListener(Reanudar);
        if (botonSalir != null) botonSalir.onClick.RemoveListener(SalirAlMenu);
    }
}
