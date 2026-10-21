#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class CrearMenuPausa
{
    private const string RutaFondo = "Assets/MenuPausa/FondoPausa.png";
    private const float Ancho = 1505f;
    private const float Alto = 1045f;

    [MenuItem("Tools/Ritmo/Crear menu de pausa")]
    public static void Crear()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Menu de pausa", "Sal del modo Play para crear el menu.", "Aceptar");
            return;
        }

#if UNITY_2022_2_OR_NEWER
        MenuPausa existente = Object.FindAnyObjectByType<MenuPausa>();
#else
        MenuPausa existente = Object.FindObjectOfType<MenuPausa>();
#endif
        if (existente != null)
        {
            Selection.activeGameObject = existente.gameObject;
            EditorUtility.DisplayDialog("Menu de pausa", "La escena ya tiene un MenuPausa. Lo seleccione en la Hierarchy.", "Aceptar");
            return;
        }

        TextureImporter importador = AssetImporter.GetAtPath(RutaFondo) as TextureImporter;
        if (importador == null)
        {
            EditorUtility.DisplayDialog("Falta el fondo", "Copia la carpeta MenuPausa completa dentro de Assets, incluyendo FondoPausa.png.", "Aceptar");
            return;
        }
        importador.textureType = TextureImporterType.Sprite;
        importador.spriteImportMode = SpriteImportMode.Single;
        importador.alphaIsTransparency = true;
        importador.SaveAndReimport();
        Sprite fondo = AssetDatabase.LoadAssetAtPath<Sprite>(RutaFondo);
        if (fondo == null)
        {
            EditorUtility.DisplayDialog("Fondo no disponible", "Espera a que Unity termine de importar FondoPausa.png e intentalo de nuevo.", "Aceptar");
            return;
        }

        Undo.IncrementCurrentGroup();
        int grupo = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Crear menu de pausa");

        GameObject raiz = new GameObject("CanvasPausa", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MenuPausa));
        raiz.layer = 5;
        Undo.RegisterCreatedObjectUndo(raiz, "Crear menu de pausa");
        Canvas canvas = raiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30000;
        CanvasScaler escalador = raiz.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(Ancho, Alto);
        escalador.matchWidthOrHeight = 0.5f;

        RectTransform panel = Rect("PanelPausa", raiz.transform);
        Estirar(panel);
        Image oscurecer = panel.gameObject.AddComponent<Image>();
        oscurecer.color = new Color(0f, 0f, 0f, 0.9f);

        RectTransform contenido = Rect("Fondo", panel);
        contenido.anchorMin = contenido.anchorMax = new Vector2(0.5f, 0.5f);
        contenido.sizeDelta = new Vector2(Ancho, Alto);
        AspectRatioFitter proporcion = contenido.gameObject.AddComponent<AspectRatioFitter>();
        proporcion.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        proporcion.aspectRatio = Ancho / Alto;
        Image imagen = contenido.gameObject.AddComponent<Image>();
        imagen.sprite = fondo;
        imagen.raycastTarget = false;

        Font fuente;
#if UNITY_2022_2_OR_NEWER
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        fuente = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        MenuPausa controlador = raiz.GetComponent<MenuPausa>();
        controlador.panelPausa = panel.gameObject;
        controlador.barraMusica = Barra(contenido, "BarraMusica", 465f,
            new Color(0f, 0.85f, 0.85f), fuente, out Text textoMusica);
        controlador.barraEfectos = Barra(contenido, "BarraEfectos", 550f,
            new Color(1f, 0.12f, 0.22f), fuente, out Text textoEfectos);
        controlador.porcentajeMusica = textoMusica;
        controlador.porcentajeEfectos = textoEfectos;
        controlador.pantallaCompleta = Casilla(contenido);
        controlador.botonReanudar = Boton(contenido, "Reanudar", 258f, 923f, 430f, 118f);
        controlador.botonSalir = Boton(contenido, "Salir", 1277f, 923f, 358f, 118f);
        Texto(contenido, "Ayuda", "ESC PARA REANUDAR", 800f, 720f, 800f, 45f, 25, fuente);

        CrearEventSystemSiFalta();
        panel.gameObject.SetActive(false);
        Selection.activeGameObject = raiz;
        EditorUtility.SetDirty(controlador);
        EditorSceneManager.MarkSceneDirty(raiz.scene);
        Undo.CollapseUndoOperations(grupo);
        EditorUtility.DisplayDialog("Menu creado", "Selecciona CanvasPausa. Asigna Musica, Efectos y Escena Menu en su componente MenuPausa. Guarda la escena y pulsa Play.", "Aceptar");
    }

    private static Slider Barra(Transform padre, string nombre, float y, Color color,
        Font fuente, out Text porcentaje)
    {
        RectTransform rect = Rect(nombre, padre);
        Colocar(rect, 850f, y, 485f, 54f);
        Slider barra = rect.gameObject.AddComponent<Slider>();
        barra.minValue = 0f;
        barra.maxValue = 1f;
        barra.value = 1f;
        barra.direction = Slider.Direction.LeftToRight;

        RectTransform pista = Rect("Pista", rect);
        Estirar(pista);
        pista.offsetMin = new Vector2(0f, 15f);
        pista.offsetMax = new Vector2(0f, -15f);
        Image pistaImagen = pista.gameObject.AddComponent<Image>();
        pistaImagen.color = new Color(0.06f, 0.07f, 0.1f);

        RectTransform area = Rect("AreaRelleno", pista);
        Estirar(area);
        RectTransform relleno = Rect("Relleno", area);
        Estirar(relleno);
        Image rellenoImagen = relleno.gameObject.AddComponent<Image>();
        rellenoImagen.color = color;
        rellenoImagen.raycastTarget = false;
        barra.fillRect = relleno;

        RectTransform areaMando = Rect("AreaMando", rect);
        Estirar(areaMando);
        RectTransform mando = Rect("Mando", areaMando);
        mando.sizeDelta = new Vector2(18f, 44f);
        Image mandoImagen = mando.gameObject.AddComponent<Image>();
        mandoImagen.color = Color.white;
        barra.handleRect = mando;
        barra.targetGraphic = mandoImagen;
        barra.navigation = new Navigation { mode = Navigation.Mode.None };
        porcentaje = Texto(padre, nombre + "Porcentaje", "100%", 1160f, y, 115f, 50f, 29, fuente);
        return barra;
    }

    private static Toggle Casilla(Transform padre)
    {
        RectTransform rect = Rect("PantallaCompleta", padre);
        Colocar(rect, 1135f, 636f, 65f, 55f);
        Toggle casilla = rect.gameObject.AddComponent<Toggle>();
        RectTransform fondo = Rect("Casilla", rect);
        Estirar(fondo);
        Image fondoImagen = fondo.gameObject.AddComponent<Image>();
        fondoImagen.color = new Color(0.14f, 0.18f, 0.21f);
        RectTransform marca = Rect("Activada", fondo);
        Estirar(marca);
        marca.offsetMin = new Vector2(9f, 9f);
        marca.offsetMax = new Vector2(-9f, -9f);
        Image marcaImagen = marca.gameObject.AddComponent<Image>();
        marcaImagen.color = new Color(0f, 0.85f, 0.85f);
        marcaImagen.raycastTarget = false;
        casilla.targetGraphic = fondoImagen;
        casilla.graphic = marcaImagen;
        casilla.isOn = Screen.fullScreen;
        casilla.navigation = new Navigation { mode = Navigation.Mode.None };
        return casilla;
    }

    private static Button Boton(Transform padre, string nombre, float x, float y, float ancho, float alto)
    {
        RectTransform rect = Rect(nombre, padre);
        Colocar(rect, x, y, ancho, alto);
        Image imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = Color.white;
        Button boton = rect.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;
        ColorBlock colores = boton.colors;
        colores.normalColor = Color.clear;
        colores.highlightedColor = new Color(1f, 1f, 1f, 0.12f);
        colores.pressedColor = new Color(0f, 0f, 0f, 0.25f);
        colores.selectedColor = Color.clear;
        boton.colors = colores;
        boton.navigation = new Navigation { mode = Navigation.Mode.None };
        return boton;
    }

    private static Text Texto(Transform padre, string nombre, string contenido,
        float x, float y, float ancho, float alto, int tamano, Font fuente)
    {
        RectTransform rect = Rect(nombre, padre);
        Colocar(rect, x, y, ancho, alto);
        Text texto = rect.gameObject.AddComponent<Text>();
        texto.font = fuente;
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.resizeTextForBestFit = true;
        texto.resizeTextMinSize = 12;
        texto.resizeTextMaxSize = tamano;
        texto.fontStyle = FontStyle.Bold;
        texto.color = Color.white;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.raycastTarget = false;
        return texto;
    }

    private static RectTransform Rect(string nombre, Transform padre)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform));
        objeto.layer = 5;
        objeto.transform.SetParent(padre, false);
        return objeto.GetComponent<RectTransform>();
    }

    // Coordenadas sobre la imagen original, con origen arriba a la izquierda.
    private static void Colocar(RectTransform rect, float x, float y, float ancho, float alto)
    {
        rect.anchorMin = new Vector2((x - ancho / 2f) / Ancho, 1f - (y + alto / 2f) / Alto);
        rect.anchorMax = new Vector2((x + ancho / 2f) / Ancho, 1f - (y - alto / 2f) / Alto);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void Estirar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void CrearEventSystemSiFalta()
    {
#if UNITY_2022_2_OR_NEWER
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
#else
        if (Object.FindObjectOfType<EventSystem>() != null) return;
#endif
        GameObject sistema = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        sistema.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        sistema.AddComponent<StandaloneInputModule>();
#endif
        Undo.RegisterCreatedObjectUndo(sistema, "Crear EventSystem");
    }
}
#endif
