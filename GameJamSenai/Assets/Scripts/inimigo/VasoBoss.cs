using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class VasoBoss : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D colisor;
    public Color corAtivo = Color.blue;
    public bool vulneravel = false;

    [Header("Configurações do Vaso Final / Captura")]
    public bool ehOUltimoVaso = false;
    public bool permiteCapturaComE = false; // Marque como true no vaso/situação de captura
    public GameObject painelCapturaUI;

    [Header("Troca de Cena pós-Captura")]
    public int indiceProximaCena = 0; // O índice da cena para onde o jogo deve ir

    [Header("Referências do Jogador (para Travar Comandos)")]
    public MonoBehaviour scriptMovimentoJogador; // Arraste o Movimento2D do Player
    public MonoBehaviour scriptAtaqueJogador;    // Arraste o AtaqueJogador do Player (se houver)

    [Header("Visual do Vaso Espatifado")]
    public Sprite spriteEspatifado;

    [Header("Efeito de Partículas")]
    public GameObject prefabParticulasEstilhaco;

    [Header("Conexão com o Fantasma")]
    public GameObject fantasma;
    public Transform pontoRessurgirDireita;

    private bool playerPerto = false;
    private bool painelAberto = false;
    private bool podeAvancarCena = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor = GetComponent<Collider2D>();

        if (colisor != null)
        {
            colisor.enabled = false;
        }

        if (painelCapturaUI != null)
        {
            painelCapturaUI.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Interação para abrir o painel com a tecla 'E'
        if (vulneravel && permiteCapturaComE && playerPerto && !painelAberto && Input.GetKeyDown(KeyCode.E))
        {
            AbrirPainelCaptura();
        }

        // 2. Se o painel estiver aberto e a transição liberada, detecta qualquer entrada (Mouse, J ou Z)
        if (painelAberto && podeAvancarCena)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z))
            {
                AvancarParaProximaCena();
            }
        }
    }

    public void AbrirPainelCaptura()
    {
        painelAberto = true;
        podeAvancarCena = false;

        if (painelCapturaUI != null)
        {
            painelCapturaUI.SetActive(true);
        }

        // Trava os comandos do jogador
        TravarJogador(true);

        // Aguarda meio segundo antes de aceitar os cliques para não passar a cena direto se o jogador clicou rápido
        StartCoroutine(LiberarAvanco());
    }

    private IEnumerator LiberarAvanco()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        podeAvancarCena = true;
    }

    public void FecharPainelCaptura()
    {
        painelAberto = false;
        podeAvancarCena = false;

        if (painelCapturaUI != null)
        {
            painelCapturaUI.SetActive(false);
        }

        // Devolve o controle ao jogador
        TravarJogador(false);
    }

    public void AvancarParaProximaCena()
    {
        // Restaura a escala de tempo
        Time.timeScale = 1f;

        // Muda para a próxima cena
        SceneManager.LoadScene(indiceProximaCena);
    }

    private void TravarJogador(bool travar)
    {
        if (scriptMovimentoJogador != null)
        {
            scriptMovimentoJogador.enabled = !travar;
        }

        if (scriptAtaqueJogador != null)
        {
            scriptAtaqueJogador.enabled = !travar;
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
                    if (pontoRessurgirDireita != null)
                    {
                        fantasma.transform.position = pontoRessurgirDireita.position;
                    }

                    fantasma.SetActive(true);

                    ComportamentoFantasma scriptFantasma = fantasma.GetComponent<ComportamentoFantasma>();
                    if (scriptFantasma != null)
                    {
                        scriptFantasma.IniciarInstabilidadeFinal();
                    }
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

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            playerPerto = true;
        }
    }

    private void OnTriggerExit2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            playerPerto = false;
        }
    }
}