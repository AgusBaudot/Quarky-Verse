using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para gestionar escenas

public class SceneChanger : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("El índice de la escena en el Build Settings")]
    public int sceneIndex;

    [Header("Detección")]
    [Tooltip("El Tag del objeto que debe activar el cambio (ej: 'Player')")]
    public string targetTag = "Player";

    // Se activa cuando otro collider entra en el área (el collider debe tener 'Is Trigger' marcado)
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que entró tiene el Tag correcto
        if (other.CompareTag(targetTag))
        {
            ChangeScene();
        }
    }

    // Método para ejecutar el cambio
    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneIndex);
    }
}