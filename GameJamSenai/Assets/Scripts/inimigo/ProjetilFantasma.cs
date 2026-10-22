using UnityEngine;
using System.Collections;

public class ProjetilFantasma : MonoBehaviour
{
    public float velocidade = 8f;
    public float tempoVida = 5f;
    public int dano = 1;

    [Header("Efeito de Debuff")]
    public float multiplicadorLentidao = 0.5f;
    public float duracaoLentidao = 5f;

    private Vector3 direcaoMovimento = Vector3.right;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        AjustarOrientacao();
        StartCoroutine(MoverEDestruir());
    }

    void AjustarOrientacao()
    {
        if (direcaoMovimento == Vector3.left)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
        else if (direcaoMovimento == Vector3.down)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90f);
        }
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
            outro.SendMessage("TomarDano", dano, SendMessageOptions.DontRequireReceiver);

            Movimento2D movimentoPlayer = outro.GetComponentInParent<Movimento2D>();

            if (movimentoPlayer != null)
            {
                movimentoPlayer.AplicarLentidao(multiplicadorLentidao, duracaoLentidao);
            }
            else
            {
                outro.SendMessageUpwards("AplicarLentidaoNoPlayer", SendMessageOptions.DontRequireReceiver);
            }

            Destroy(gameObject);
        }
    }
}