using UnityEngine;

public enum TipoNota
{
    Normal,
    CambioCarril,
    DobleGolpe,
    ParryFever
}

public class NoteObject : MonoBehaviour
{
    public float beatOfThisNote;
    public Conductor conductor;
    public Vector3 startPos;
    public Vector3 endPos;
    public bool fueGolpeada = false;

    public TipoNota tipo = TipoNota.Normal;
    public int carrilActual;
    public int golpesRestantes = 1;

    private bool enRebote = false;
    private float beatInicioRebote;
    private float duracionReboteBeats;
    private Vector3 posImpacto;
    private Vector3 posCumbre;
    private SpriteRenderer sr;

    public void Initialize(float beat, Conductor cond, int lane, TipoNota tipoInicial)
    {
        beatOfThisNote = beat;
        conductor = cond;
        carrilActual = lane;
        tipo = tipoInicial;

        startPos = cond.spawnPoints[lane].position;
        endPos = cond.hitZones[lane].position;
        transform.position = startPos;

        golpesRestantes = (tipo == TipoNota.CambioCarril || tipo == TipoNota.DobleGolpe) ? 2 : 1;

        sr = GetComponent<SpriteRenderer>();
        ActualizarColor();
    }

    public void ActivarRebote(int nuevoCarril, float beatsExtra, float alturaRetorno)
    {
        enRebote = true;
        fueGolpeada = false;
        beatInicioRebote = conductor.songPositionInBeats;
        duracionReboteBeats = beatsExtra;
        beatOfThisNote = beatInicioRebote + beatsExtra;

        posImpacto = transform.position;
        carrilActual = nuevoCarril;
        startPos = conductor.spawnPoints[nuevoCarril].position;
        endPos = conductor.hitZones[nuevoCarril].position;
        posCumbre = Vector3.Lerp(startPos, endPos, alturaRetorno);

        tipo = TipoNota.Normal;
        ActualizarColor();
    }

    void ActualizarColor()
    {
        if (sr == null) return;

        switch (tipo)
        {
            case TipoNota.Normal:
                sr.color = Color.white;
                break;
            case TipoNota.CambioCarril:
                sr.color = Color.magenta;
                break;
            case TipoNota.DobleGolpe:
                sr.color = new Color(0.2f, 0.6f, 1f);
                break;
            case TipoNota.ParryFever:
                sr.color = Color.yellow;
                break;
        }
    }

    void Update()
    {
        if (!enRebote)
        {
            float beatsFromHit = beatOfThisNote - conductor.songPositionInBeats;
            float interpolation = 1f - (beatsFromHit / conductor.beatsAdvance);
            transform.position = Vector3.LerpUnclamped(startPos, endPos, interpolation);

            if (interpolation > 1.2f)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            float progreso = (conductor.songPositionInBeats - beatInicioRebote) / duracionReboteBeats;

            if (progreso < 0.5f)
            {
                transform.position = Vector3.Lerp(posImpacto, posCumbre, progreso * 2f);
            }
            else
            {
                transform.position = Vector3.LerpUnclamped(posCumbre, endPos, (progreso - 0.5f) * 2f);
            }

            if (progreso > 1.2f)
            {
                Destroy(gameObject);
            }
        }
    }
}
