using UnityEngine;
using System.Collections.Generic;

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

    public TextAsset archivoOsu; 
    public GameObject notePrefab;
    public Transform[] spawnPoints;
    public Transform[] hitZones;
    public List<Note> mapNotes = new List<Note>();
    public int currentNoteIndex = 0;

    [System.Serializable]
    public struct Note
    {
        public float targetBeat;
        public int laneIndex;
    }

    void Start()
    {
        secPerBeat = 60f / bpm;
        CargarMapaOsu(); // Genera la lista antes de que empiece la música

        dspSongTime = (float)AudioSettings.dspTime;
        musicSource.Play();
    }

    void CargarMapaOsu()
    {
        mapNotes.Clear();
        string[] lineas = archivoOsu.text.Split('\n');
        bool leyendoNotas = false;

        foreach (string linea in lineas)
        {
            // Busca dónde empiezan las notas en el archivo
            if (linea.Trim() == "[HitObjects]")
            {
                leyendoNotas = true;
                continue;
            }

            if (leyendoNotas && !string.IsNullOrWhiteSpace(linea))
            {
                // osu! guarda los datos así: x,y,tiempoEnMs,tipo,sonido...
                string[] datos = linea.Split(',');

                if (datos.Length >= 3)
                {
                    // 1. Calcular el carril basado en la posición X (osu! usa un ancho de 0 a 512)
                    float x = float.Parse(datos[0]);
                    int carril = 0;
                    if (x > 170f) carril = 1; // Centro
                    if (x > 341f) carril = 2; // Derecha

                    // 2. Convertir el tiempo de milisegundos a Beats para nuestro sistema
                    float tiempoEnSegundos = float.Parse(datos[2]) / 1000f;
                    float beatExacto = tiempoEnSegundos / secPerBeat;

                    // 3. Añadir a la lista
                    mapNotes.Add(new Note { targetBeat = beatExacto, laneIndex = carril });
                }
            }
        }
    }

    void Update()
    {
        songPosition = (float)(AudioSettings.dspTime - dspSongTime);
        songPositionInBeats = songPosition / secPerBeat;

        if (currentNoteIndex < mapNotes.Count)
        {
            if (mapNotes[currentNoteIndex].targetBeat < songPositionInBeats + beatsAdvance)
            {
                SpawnNote(mapNotes[currentNoteIndex].laneIndex, mapNotes[currentNoteIndex].targetBeat);
                currentNoteIndex++;
            }
        }
    }

    void SpawnNote(int lane, float targetBeat)
    {
        GameObject note = Instantiate(notePrefab);
        NoteObject noteScript = note.GetComponent<NoteObject>();
        noteScript.Initialize(targetBeat, this, spawnPoints[lane].position, hitZones[lane].position);
    }
}
