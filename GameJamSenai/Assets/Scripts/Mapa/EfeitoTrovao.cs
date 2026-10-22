using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EfeitoTrovao : MonoBehaviour
{
    public AudioClip somTrovao;
    private AudioSource audioSource;

    public GameObject painelPreto;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    public void DispararTrovao()
    {
        StartCoroutine(RotinaTrovao());
    }

    private IEnumerator RotinaTrovao()
    {
        if (audioSource != null && somTrovao != null)
        {
            audioSource.PlayOneShot(somTrovao);
        }

        if (painelPreto != null)
        {
            painelPreto.SetActive(true);
            DefinirAlphaPainelPreto(1.0f);

            float[] intervalos = new float[] { 0.05f, 0.05f, 0.08f, 0.1f, 0.2f, 0.4f, 0.7f };

            for (int i = 0; i < intervalos.Length; i++)
            {
                painelPreto.SetActive(true);
                yield return new WaitForSeconds(intervalos[i]);

                painelPreto.SetActive(false);
                yield return new WaitForSeconds(intervalos[i]);
            }

            painelPreto.SetActive(false);
        }
    }

    private void DefinirAlphaPainelPreto(float alpha)
    {
        if (painelPreto != null)
        {
            Image img = painelPreto.GetComponent<Image>();
            if (img != null)
            {
                Color cor = img.color;
                cor.a = alpha;
                img.color = cor;
            }
        }
    }
}