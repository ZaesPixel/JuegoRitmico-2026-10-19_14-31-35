using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Conductor : MonoBehaviour
{
    public float bpm;
    public float secPerBeat;
    public float songPosition;
    public float songPositionInBeats;
    public float dspSongTime;
    public AudioSource musicSource;
    public float beatsAdvance = 4f;
    public float offset = 0f;
    public float musicStartDelay = 3f;

    public TextAsset mapaFacil;
    public TextAsset mapaDificil;
    public bool jugarEnDificil = false;

    public GameObject notePrefab;
    public GameObject prefabCambioCarril;
    public GameObject prefabDobleGolpe;
    public GameObject prefabParryFever;

    public Transform[] spawnPoints;
    public Transform[] hitZones;
    public List<Note> mapNotes = new List<Note>();
    public int currentNoteIndex = 0;

    public int puntuacion = 0;
    public int combo = 0;

    public TMP_Text textoPuntos;
    public TMP_Text textoCombo;
    public TMP_Text textoCalificacion;

    public Slider barraFever;
    public TMP_Text textoFever;
    public float feverActual = 0f;
    public float feverMaximo = 300f;
    public float duracionFever = 6f;
    public float cargaPerfect = 10f;
    public float cargaGreat = 5f;
    public bool enFever = false;
    private float tiempoRestanteFever = 0f;

    public int frecuenciaDobleGolpe = 5;
    public int frecuenciaCambioCarril = 8;
    public int frecuenciaParryEnFever = 3;

    [System.Serializable]
    public struct Note
    {
        public float targetBeat;
        public int laneIndex;
        public TipoNota tipo;
    }

    void Start()
    {
        if (frecuenciaDobleGolpe <= 0) frecuenciaDobleGolpe = 5;
        if (frecuenciaCambioCarril <= 0) frecuenciaCambioCarril = 8;
        if (frecuenciaParryEnFever <= 0) frecuenciaParryEnFever = 3;

        secPerBeat = 60f / bpm;
        CargarMapaOsu();
        dspSongTime = (float)AudioSettings.dspTime + musicStartDelay;
        musicSource.PlayScheduled(dspSongTime);

        if (barraFever != null)
        {
            barraFever.maxValue = feverMaximo;
            barraFever.value = 0f;
        }

        if (textoFever != null)
        {
            textoFever.text = "";
        }

        ActualizarUI("", Color.white);
    }

    void CargarMapaOsu()
    {
        mapNotes.Clear();
        TextAsset archivoAUsar = jugarEnDificil ? mapaDificil : mapaFacil;
        string[] lineas = archivoAUsar.text.Split('\n');
        bool leyendoNotas = false;
        int contadorNotas = 0;

        foreach (string linea in lineas)
        {
            if (linea.Trim() == "[HitObjects]")
            {
                leyendoNotas = true;
                continue;
            }

            if (leyendoNotas && !string.IsNullOrWhiteSpace(linea))
            {
                string[] datos = linea.Split(',');
                if (datos.Length >= 3)
                {
                    contadorNotas++;
                    float x = float.Parse(datos[0]);
                    int carril = 0;
                    if (x > 170f) carril = 1;
                    if (x > 341f) carril = 2;

                    float tiempoEnSegundos = float.Parse(datos[2]) / 1000f;
                    float beatExacto = tiempoEnSegundos / secPerBeat;

                    TipoNota tipoAsignado = TipoNota.Normal;

                    if (frecuenciaCambioCarril > 0 && contadorNotas % frecuenciaCambioCarril == 0)
                    {
                        tipoAsignado = TipoNota.CambioCarril;
                    }
                    else if (frecuenciaDobleGolpe > 0 && contadorNotas % frecuenciaDobleGolpe == 0)
                    {
                        tipoAsignado = TipoNota.DobleGolpe;
                    }

                    mapNotes.Add(new Note { targetBeat = beatExacto, laneIndex = carril, tipo = tipoAsignado });
                }
            }
        }
    }

    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime) - offset;
        songPositionInBeats = songPosition / secPerBeat;

        if (currentNoteIndex < mapNotes.Count)
        {
            if (mapNotes[currentNoteIndex].targetBeat < songPositionInBeats + beatsAdvance)
            {
                TipoNota tipoFinal = mapNotes[currentNoteIndex].tipo;

                if (enFever && frecuenciaParryEnFever > 0 && currentNoteIndex % frecuenciaParryEnFever == 0)
                {
                    tipoFinal = TipoNota.ParryFever;
                }

                SpawnNote(mapNotes[currentNoteIndex].laneIndex, mapNotes[currentNoteIndex].targetBeat, tipoFinal);
                currentNoteIndex++;
            }
        }

        if (enFever)
        {
            tiempoRestanteFever -= Time.deltaTime;
            feverActual = (tiempoRestanteFever / duracionFever) * feverMaximo;

            if (barraFever != null)
            {
                barraFever.value = feverActual;
            }

            if (tiempoRestanteFever <= 0f)
            {
                enFever = false;
                feverActual = 0f;

                if (barraFever != null)
                {
                    barraFever.value = 0f;
                }

                if (textoFever != null)
                {
                    textoFever.text = "";
                }
            }
        }
    }

    void SpawnNote(int lane, float targetBeat, TipoNota tipo)
    {
        GameObject prefabAUsar = notePrefab;

        if (tipo == TipoNota.CambioCarril && prefabCambioCarril != null)
        {
            prefabAUsar = prefabCambioCarril;
        }
        else if (tipo == TipoNota.DobleGolpe && prefabDobleGolpe != null)
        {
            prefabAUsar = prefabDobleGolpe;
        }
        else if (tipo == TipoNota.ParryFever && prefabParryFever != null)
        {
            prefabAUsar = prefabParryFever;
        }

        GameObject note = Instantiate(prefabAUsar);
        NoteObject noteScript = note.GetComponent<NoteObject>();
        noteScript.Initialize(targetBeat, this, lane, tipo);
    }

    public void SumarPuntos(string calificacion, int puntosBase)
    {
        combo++;
        int multiplicadorFever = enFever ? 2 : 1;
        puntuacion += puntosBase * combo * multiplicadorFever;

        if (!enFever)
        {
            if (calificacion == "PERFECT")
            {
                feverActual += cargaPerfect;
            }
            else if (calificacion == "GREAT")
            {
                feverActual += cargaGreat;
            }

            if (feverActual >= feverMaximo)
            {
                ActivarFever();
            }
            else if (barraFever != null)
            {
                barraFever.value = feverActual;
            }
        }

        Color colorTexto = Color.green;
        if (calificacion == "PERFECT") colorTexto = Color.cyan;
        else if (calificacion == "PARRY!") colorTexto = Color.yellow;

        string textoMostrar = enFever ? calificacion + " x2" : calificacion;
        ActualizarUI(textoMostrar, colorTexto);
    }

    void ActivarFever()
    {
        enFever = true;
        feverActual = feverMaximo;
        tiempoRestanteFever = duracionFever;

        if (barraFever != null)
        {
            barraFever.value = feverMaximo;
        }

        if (textoFever != null)
        {
            textoFever.text = "FEVER";
            textoFever.color = Color.yellow;
        }
    }

    public void RegistrarMiss()
    {
        combo = 0;
        ActualizarUI("MISS", Color.red);
    }

    void ActualizarUI(string calificacion, Color colorCalificacion)
    {
        if (textoPuntos != null) textoPuntos.text = "Puntos: " + puntuacion;
        if (textoCombo != null) textoCombo.text = combo > 0 ? "Combo: x" + combo : "";
        if (textoCalificacion != null)
        {
            textoCalificacion.text = calificacion;
            textoCalificacion.color = colorCalificacion;
            CancelInvoke("LimpiarCalificacion");
            Invoke("LimpiarCalificacion", 0.5f);
        }
    }

    void LimpiarCalificacion()
    {
        if (textoCalificacion != null) textoCalificacion.text = "";
    }
}
