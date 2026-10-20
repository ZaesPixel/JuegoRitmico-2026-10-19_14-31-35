using UnityEngine;

public class BotonRitmo : MonoBehaviour
{
    public KeyCode teclaAsignada;
    private SpriteRenderer sr;
    public Color colorPresionado = new Color(0.5f, 0.5f, 0.5f, 1f);
    private Color colorOriginal;

    public float ventanaPerfect = 0.08f;
    public float ventanaGreat = 0.18f;

    private bool puedePresionar = false;
    private NoteObject notaActual;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        colorOriginal = sr.color;
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaAsignada))
        {
            sr.color = colorPresionado;

            if (puedePresionar && notaActual != null && notaActual.tipo != TipoNota.ParryFever)
            {
                EvaluarTiempo(false);
            }
        }

        if (Input.GetKeyUp(teclaAsignada))
        {
            sr.color = colorOriginal;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (puedePresionar && notaActual != null && notaActual.tipo == TipoNota.ParryFever)
            {
                EvaluarTiempo(true);
            }
        }
    }

    void EvaluarTiempo(bool esParry)
    {
        Conductor cond = notaActual.conductor;
        float tiempoExactoNota = notaActual.beatOfThisNote * cond.secPerBeat;
        float diferenciaTiempo = Mathf.Abs(cond.songPosition - tiempoExactoNota);

        if (diferenciaTiempo <= ventanaGreat)
        {
            string calificacion = diferenciaTiempo <= ventanaPerfect ? "PERFECT" : "GREAT";
            int puntos = diferenciaTiempo <= ventanaPerfect ? 100 : 50;

            if (esParry)
            {
                calificacion = "PARRY!";
                puntos = 250;
            }

            notaActual.golpesRestantes--;

            if (notaActual.golpesRestantes > 0)
            {
                if (notaActual.tipo == TipoNota.CambioCarril)
                {
                    int nuevoCarril = (notaActual.carrilActual + 1) % cond.hitZones.Length;
                    notaActual.ActivarRebote(nuevoCarril, 2f, 0.35f);
                }
                else if (notaActual.tipo == TipoNota.DobleGolpe)
                {
                    notaActual.ActivarRebote(notaActual.carrilActual, 1f, 0.6f);
                }

                cond.SumarPuntos(calificacion, puntos);
                puedePresionar = false;
                notaActual = null;
            }
            else
            {
                notaActual.fueGolpeada = true;
                cond.SumarPuntos(calificacion, puntos);
                Destroy(notaActual.gameObject);
                puedePresionar = false;
                notaActual = null;
            }
        }
        else
        {
            notaActual.fueGolpeada = true;
            cond.RegistrarMiss();
            Destroy(notaActual.gameObject);
            puedePresionar = false;
            notaActual = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        NoteObject notaEntrante = collision.GetComponent<NoteObject>();
        if (notaEntrante != null)
        {
            puedePresionar = true;
            notaActual = notaEntrante;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        NoteObject notaSaliente = collision.GetComponent<NoteObject>();
        if (notaSaliente != null)
        {
            if (!notaSaliente.fueGolpeada && notaSaliente.conductor.songPositionInBeats > notaSaliente.beatOfThisNote)
            {
                notaSaliente.conductor.RegistrarMiss();
            }

            if (notaActual == notaSaliente)
            {
                puedePresionar = false;
                notaActual = null;
            }
        }
    }
}
