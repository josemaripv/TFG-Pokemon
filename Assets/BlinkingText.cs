using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlinkingText : MonoBehaviour
{
    public TMP_Text textToBlink;  // Arrastra el objeto de texto aquí en el inspector
    public float blinkDuration = 0.5f;  // Duración del parpadeo en segundos

    private void Start()
    {
        if (textToBlink == null)
        {
            textToBlink = GetComponent<TMP_Text>();
        }
        StartCoroutine(Blink());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))  // Aquí puedes cambiar KeyCode.Return por la tecla que desees usar
        {
            LoadMainScene();
        }
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            textToBlink.enabled = !textToBlink.enabled;
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene("PuebloPrincipal");
    }
}
