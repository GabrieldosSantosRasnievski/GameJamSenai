using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class VidaJogador : MonoBehaviour
{
    public int vidaMaxima = 5;
    private int vidaAtual;

    [Header("UI da Barra de Vida")]
    public Image barraDeVida;               // A imagem única da barra de vida no Canvas
    public Sprite[] spritesVida;            // Array de sprites (Tamanho 6: do índice 0 ao 5)

    public float tempoInvencibilidade = 1.5f;
    private bool estaInvencivel = false;

    public SpriteRenderer spriteJogador;
    public float tempoPisca = 0.1f;

    [Header("UI do Painel de Morte")]
    public GameObject painelGameOver;
    public TextMeshProUGUI textoMensagemStatus;

    void Start()
    {
        Time.timeScale = 1f;
        vidaAtual = vidaMaxima;
        
        if (spriteJogador == null)
        {
            spriteJogador = GetComponent<SpriteRenderer>();
        }

        if (painelGameOver != null)
        {
            painelGameOver.SetActive(false);
        }

        AtualizarUI();
    }

    public void TomarDano(int dano)
    {
        if (estaInvencivel)
        {
            return;
        }

        vidaAtual -= dano;
        vidaAtual = Mathf.Clamp(vidaAtual, 0, vidaMaxima);

        AtualizarUI();

        if (vidaAtual <= 0)
        {
            Morrer();
        }
        else
        {
            StartCoroutine(RotinaInvencibilidade());
        }
    }

    private IEnumerator RotinaInvencibilidade()
    {
        estaInvencivel = true;
        float tempoDecorridos = 0f;

        while (tempoDecorridos < tempoInvencibilidade)
        {
            if (spriteJogador != null)
            {
                spriteJogador.enabled = !spriteJogador.enabled;
            }
            
            yield return new WaitForSeconds(tempoPisca);
            tempoDecorridos += tempoPisca;
        }

        if (spriteJogador != null)
        {
            spriteJogador.enabled = true;
        }

        estaInvencivel = false;
    }

    void AtualizarUI()
    {
        // Garante que o Sprite seja alterado de acordo com o valor exato da vidaAtual
        if (barraDeVida != null && spritesVida != null && spritesVida.Length > vidaAtual)
        {
            barraDeVida.sprite = spritesVida[vidaAtual];
        }
    }

    void Morrer()
    {
        Debug.Log("O jogador morreu!");

        // Busca o script do Fantasma para saber em qual fase ele parou
        ComportamentoFantasma fantasma = FindObjectOfType<ComportamentoFantasma>();
        int faseAtualDoBoss = (fantasma != null) ? fantasma.faseAtual : 1;

        if (painelGameOver != null)
        {
            painelGameOver.SetActive(true);
            ExibirMensagemStatus(faseAtualDoBoss);
        }

        Time.timeScale = 0f;
        gameObject.SetActive(false);
    }

    void ExibirMensagemStatus(int faseBoss)
    {
        if (textoMensagemStatus != null)
        {
            switch (faseBoss)
            {
                case 1:
                    textoMensagemStatus.text = "Passou longe!"; // Morreu antes de quebrar o Vaso 1
                    break;
                case 2:
                    textoMensagemStatus.text = "Tá pegando o jeito!"; // Morreu no Vaso 1 / transição pro Vaso 2
                    break;
                case 3:
                    textoMensagemStatus.text = "Tão perto!"; // Morreu perto de vencer o Boss no Vaso 2
                    break;
                default:
                    textoMensagemStatus.text = "Você conseguiu!";
                    break;
            }
        }
    }

    public void ReiniciarCena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CarregarProximaCena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}