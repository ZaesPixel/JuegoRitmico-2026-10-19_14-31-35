using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    public void AbrirPersonajes()
    {
        SceneManager.LoadScene("Personajes");
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Juego");
    }

    public void AbrirAjustes()
    {
        SceneManager.LoadScene("Ajustes");
    }
}