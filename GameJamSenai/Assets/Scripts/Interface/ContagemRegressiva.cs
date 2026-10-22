using UnityEngine;
using TMPro;
using System.Collections;

public class ContagemRegressiva : MonoBehaviour
{
    [Header("Interface da Contagem")]
    public TextMeshProUGUI textoContagem;

    [Header("Áudio (Opcional)")]
    public AudioSource audioSource;
    public AudioClip somBip;
    public AudioClip somInicio;

    [Header("Referências do Jogador")]
    public MonoBehaviour scriptMovimentoJogador; // Arraste o script de controle do jogador (ex: Movimento2D)

    void Start()
    {
        StartCoroutine(RotinaContagem());
    }

    private IEnumerator RotinaContagem()
    {
        // 1. Pausa o tempo do jogo
        Time.timeScale = 0f;

        // 2. Desativa o script do jogador para evitar comandos (ataques, pulos, etc)
        if (scriptMovimentoJogador != null)
        {
            scriptMovimentoJogador.enabled = false;
        }

        if (textoContagem != null)
        {
            textoContagem.gameObject.SetActive(true);
        }

        // 3. Loop da contagem de 3 a 1
        for (int i = 3; i >= 1; i--)
        {
            if (textoContagem != null)
            {
                textoContagem.text = i.ToString();
            }

            if (audioSource != null && somBip != null)
            {
                audioSource.PlayOneShot(somBip);
            }

            // Usa WaitForSecondsRealtime porque o Time.timeScale está em 0!
            yield return new WaitForSecondsRealtime(1.0f);
        }

        // 4. Exibe a mensagem de início
        if (textoContagem != null)
        {
            textoContagem.text = "LUTE!";
        }

        if (audioSource != null && somInicio != null)
        {
            audioSource.PlayOneShot(somInicio);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        // 5. Oculta o texto e descongela o jogo
        if (textoContagem != null)
        {
            textoContagem.gameObject.SetActive(false);
        }

        Time.timeScale = 1f;

        // 6. Reativa os controles do jogador
        if (scriptMovimentoJogador != null)
        {
            scriptMovimentoJogador.enabled = true;
        }
    }
}