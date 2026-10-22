using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GerenciamentoMenu : MonoBehaviour
{
    [Header("Áudio da Bruxa")]
    public AudioSource audioSource;
    public AudioClip somBruxa;

    [Header("Efeito de Escurecimento (Fade)")]
    [Tooltip("Arraste uma Image preta que cubra toda a tela")]
    public Image imagemFadePreto;

    void Start()
    {
        // Garante que a tela comece transparente
        if (imagemFadePreto != null)
        {
            Color cor = imagemFadePreto.color;
            cor.a = 0f;
            imagemFadePreto.color = cor;
            imagemFadePreto.gameObject.SetActive(false);
        }
    }

    public void IrParaTelaDeCarregamento(int indiceDaCena)
    {
        StartCoroutine(TrocarDeCenaAposAudioEFade(indiceDaCena));
    }

    private IEnumerator TrocarDeCenaAposAudioEFade(int indiceDaCena)
    {
        // 1. Inicia o carregamento assíncrono em segundo plano
        AsyncOperation operacao = SceneManager.LoadSceneAsync(indiceDaCena);
        operacao.allowSceneActivation = false;

        float duracaoAudio = 0f;

        // 2. Toca o som
        if (audioSource != null && somBruxa != null)
        {
            audioSource.PlayOneShot(somBruxa);
            duracaoAudio = somBruxa.length; // ~4.35s
        }

        // 3. Ativa a imagem de fade
        if (imagemFadePreto != null)
        {
            imagemFadePreto.gameObject.SetActive(true);
        }

        // 4. Conforme o áudio toca, a tela vai escurecendo gradualmente
        float tempoDecorrido = 0f;

        while (tempoDecorrido < duracaoAudio)
        {
            tempoDecorrido += Time.deltaTime;

            if (imagemFadePreto != null && duracaoAudio > 0f)
            {
                Color cor = imagemFadePreto.color;
                // Calcula a opacidade de 0 (transparente) até 1 (totalmente preto) com base no tempo do áudio
                cor.a = Mathf.Clamp01(tempoDecorrido / duracaoAudio);
                imagemFadePreto.color = cor;
            }

            yield return null;
        }

        // Garante que a tela fique 100% preta no final
        if (imagemFadePreto != null)
        {
            Color cor = imagemFadePreto.color;
            cor.a = 1f;
            imagemFadePreto.color = cor;
        }

        // 5. Libera a troca de cena
        operacao.allowSceneActivation = true;
    }
}