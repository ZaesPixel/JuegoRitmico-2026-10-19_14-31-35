using UnityEngine;

public class BotonRitmo : MonoBehaviour
{
    public KeyCode teclaAsignada;
    private SpriteRenderer sr;
    public Color colorPresionado = new Color(0.5f, 0.5f, 0.5f, 1f);
    private Color colorOriginal;

    private bool puedePresionar = false;
    private GameObject notaActual;

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

            if (puedePresionar && notaActual != null)
            {
                EvaluarPrecision();
            }
        }

        if (Input.GetKeyUp(teclaAsignada))
        {
            sr.color = colorOriginal;
        }
    }

    void EvaluarPrecision()
    {
        // Calculamos la distancia vertical (eje Y) entre la nota y este botón
        float distancia = Mathf.Abs(notaActual.transform.position.y - transform.position.y);

        if (distancia <= 0.2f)
        {
            Debug.Log("<color=cyan>¡PERFECTO!</color>");
            // Aquí sumaremos 100 puntos
        }
        else if (distancia <= 0.5f)
        {
            Debug.Log("<color=green>¡BIEN!</color>");
            // Aquí sumaremos 50 puntos
        }
        else
        {
            Debug.Log("<color=red>¡FALLO!</color>");
            // Aquí romperemos el combo
        }

        // Destruimos la nota tras golpearla
        Destroy(notaActual);
        puedePresionar = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Nota"))
        {
            puedePresionar = true;
            notaActual = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Nota"))
        {
            puedePresionar = false;
            notaActual = null;
            Debug.Log("<color=red>¡DEJASTE PASAR LA NOTA!</color>");
        }
    }
}
