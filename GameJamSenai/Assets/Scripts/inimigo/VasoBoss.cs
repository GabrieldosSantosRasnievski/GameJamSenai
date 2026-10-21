using UnityEngine;

public class VasoBoss : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D colisor;
    public Color corAtivo = Color.blue;
    public bool vulneravel = false;

    [Header("Configurações do Vaso Final")]
    public bool ehOUltimoVaso = false;

    [Header("Visual do Vaso Espatifado")]
    public Sprite spriteEspatifado;

    [Header("Efeito de Partículas")]
    public GameObject prefabParticulasEstilhaco;

    [Header("Conexão com o Fantasma")]
    public GameObject fantasma;
    public Transform pontoRessurgirDireita;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor = GetComponent<Collider2D>();

        if (colisor != null)
        {
            colisor.enabled = false;
        }
    }

    public void AtivarVaso()
    {
        vulneravel = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = corAtivo;
        }

        if (colisor != null)
        {
            colisor.enabled = true;
        }
    }

    public void TomarDano(int dano)
    {
        if (vulneravel)
        {
            Debug.Log("Vaso quebrado!");

            vulneravel = false;

            if (prefabParticulasEstilhaco != null)
            {
                GameObject efeito = Instantiate(prefabParticulasEstilhaco, transform.position, Quaternion.identity);
                Destroy(efeito, 2f);
            }

            if (spriteRenderer != null)
            {
                if (spriteEspatifado != null)
                {
                    spriteRenderer.sprite = spriteEspatifado;
                }
                spriteRenderer.color = Color.white;
            }

            if (colisor != null)
            {
                colisor.enabled = false;
            }

            if (ehOUltimoVaso)
            {
                if (fantasma != null)
                {
                    fantasma.SetActive(true);
                }

                if (GerenciadorFaseFinal.Instancia != null)
                {
                    GerenciadorFaseFinal.Instancia.IniciarSequenciaFinal();
                }
            }
            else
            {
                if (fantasma != null)
                {
                    if (pontoRessurgirDireita != null)
                    {
                        fantasma.transform.position = pontoRessurgirDireita.position;
                    }

                    fantasma.SetActive(true);
                }
            }
        }
    }
}