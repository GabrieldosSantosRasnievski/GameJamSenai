using UnityEngine;
using System.Collections;

public class ProjetilFantasma : MonoBehaviour
{
    public float velocidade = 8f;
    public float tempoVida = 5f;
    public int dano = 1;

    [Header("Efeito de Debuff")]
    public float multiplicadorLentidao = 0.5f; // 50% da velocidade normal
    public float duracaoLentidao = 5f;          // 5 segundos de duração

    private Vector3 direcaoMovimento = Vector3.right;

    void Start()
    {
        ComportamentoFantasma fantasma = FindObjectOfType<ComportamentoFantasma>();

        if (fantasma != null)
        {
            if (fantasma.faseAtual == 1)
            {
                direcaoMovimento = Vector3.right;
            }
            else if (fantasma.faseAtual == 2)
            {
                direcaoMovimento = Vector3.left;
            }
            else if (fantasma.faseAtual >= 3)
            {
                direcaoMovimento = Vector3.down;
            }
        }
        else
        {
            direcaoMovimento = transform.right;
        }

        StartCoroutine(MoverEDestruir());
    }

    private IEnumerator MoverEDestruir()
    {
        float tempoDecorrido = 0f;

        while (tempoDecorrido < tempoVida)
        {
            transform.Translate(direcaoMovimento * velocidade * Time.deltaTime, Space.World);
            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            // 1. Aplica o Dano via SendMessage (funciona em qualquer script de vida)
            outro.SendMessage("TomarDano", dano, SendMessageOptions.DontRequireReceiver);

            // 2. Busca o script Movimento2D na hierarquia (objeto atual ou pai)
            Movimento2D movimentoPlayer = outro.GetComponentInParent<Movimento2D>();

            if (movimentoPlayer != null)
            {
                movimentoPlayer.AplicarLentidao(multiplicadorLentidao, duracaoLentidao);
            }
            else
            {
                // Fallback caso o script esteja em outro nível da hierarquia
                outro.SendMessageUpwards("AplicarLentidaoNoPlayer", SendMessageOptions.DontRequireReceiver);
            }

            Destroy(gameObject);
        }
    }
}