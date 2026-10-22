using UnityEngine;
using System.Collections;

public class Movimentacao2D : MonoBehaviour
{
    [SerializeField] private float velocidadeMovimento = 8f;
    [SerializeField] private float forcaDoPulo = 12f;
    [SerializeField] private Transform verificadorDeChao;
    [SerializeField] private LayerMask oQueEChao;
    [SerializeField] private float raioVerificacao = 0.2f;
    private Rigidbody2D rb;
<<<<<<< HEAD
    public float velocidadeMovimento = 8f;
    private float velocidadeAtual;
    private float entradaHorizontal;
    private bool olhandoParaDireita = true;

    [Header("Mecânica de Sução / Física Externa")]
    public bool sendoSugado = false;
    public float forcaControleNoArESucao = 25f;

    [Header("Mecânica de Lentidão")]
    public float multiplicadorLentidao = 0.5f;
    public float duracaoLentidao = 5f;
    private Coroutine corrotinaLentidao;

    public float forcaPulo = 12f;
    public Transform checadorChao;
    public LayerMask camadaChao;
    public Vector2 tamanhoChecadorChao = new Vector2(0.5f, 0.1f);
    private bool estaNoChao;

    public float tempoCoyote = 0.15f;
    private float coyoteTempoContador;

    public float gravidadePadrao = 3f;
    public float multiplicadorQueda = 2.5f;
    public float multiplicadorPuloCurto = 2f;
    public float multiplicadorQuedaRapida = 5f;

    public Transform pontoAtaque;
    public GameObject prefabAtaque;
    public float raioAtaque = 0.8f;
    public LayerMask camadaInimigos;
    public float tempoVidaAtaque = 0.2f;
    public float tempoEntreAtaques = 0.4f;
    private float proximoAtaqueTempo = 0f;
    public int danoAtaque = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravidadePadrao;
        velocidadeAtual = velocidadeMovimento;
    }

    void Update()
    {
        entradaHorizontal = Input.GetAxisRaw("Horizontal");

        VerificarVirada();

        // CHECAGEM RIGOROSA DE CHÃO
        if (checadorChao != null)
        {
            if (rb.linearVelocity.y > 0.01f)
            {
                estaNoChao = false;
            }
            else
            {
                estaNoChao = Physics2D.OverlapBox(checadorChao.position, tamanhoChecadorChao, 0f, camadaChao);
            }
        }

        // CONTROLE DO COYOTE TIME
        if (estaNoChao)
        {
            coyoteTempoContador = tempoCoyote;
        }
        else
        {
            if (rb.linearVelocity.y > 0.01f)
            {
                coyoteTempoContador = 0f;
            }
            else
            {
                coyoteTempoContador -= Time.deltaTime;
            }
        }

        bool segurandoBaixo = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool apertouPulo = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

        if (apertouPulo && coyoteTempoContador > 0f && !segurandoBaixo && rb.linearVelocity.y <= 0.01f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaPulo);
            coyoteTempoContador = 0f;
            estaNoChao = false;
        }

        AjustarGravidade();

        if (Time.time >= proximoAtaqueTempo)
        {
            bool apertouAtaque = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J);

            if (apertouAtaque)
            {
                Atacar();
                proximoAtaqueTempo = Time.time + tempoEntreAtaques;
            }
        }
    }

    void FixedUpdate()
    {
        if (sendoSugado)
        {
            // Permite acumular a força da sução sem zerar a velocidade horizontal do Rigidbody2D
            Vector2 forcaEntrada = new Vector2(entradaHorizontal * forcaControleNoArESucao, 0f);
            rb.AddForce(forcaEntrada, ForceMode2D.Force);

            // Trava a velocidade máxima de caminhada do jogador quando se move na direção oposta/a favor
            float velocidadeXLimitada = Mathf.Clamp(rb.linearVelocity.x, -velocidadeAtual * 2f, velocidadeAtual * 2f);
            rb.linearVelocity = new Vector2(velocidadeXLimitada, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(entradaHorizontal * velocidadeAtual, rb.linearVelocity.y);
        }
    }

    public void SetSendoSugado(bool valor)
    {
        sendoSugado = valor;
    }

    public void AplicarLentidaoNoPlayer()
    {
        AplicarLentidao(multiplicadorLentidao, duracaoLentidao);
    }

    public void AplicarLentidao(float mult, float duracao)
    {
        if (corrotinaLentidao != null)
        {
            StopCoroutine(corrotinaLentidao);
        }
        corrotinaLentidao = StartCoroutine(RotinaLentidao(mult, duracao));
    }

    private IEnumerator RotinaLentidao(float mult, float duracao)
    {
        velocidadeAtual = velocidadeMovimento * mult;
        yield return new WaitForSeconds(duracao);
        velocidadeAtual = velocidadeMovimento;
        corrotinaLentidao = null;
    }

    void Atacar()
    {
        if (pontoAtaque == null) return;

        if (prefabAtaque != null)
        {
            GameObject ataqueCriado = Instantiate(prefabAtaque, pontoAtaque.position, transform.rotation);
            Vector3 escalaAtaque = ataqueCriado.transform.localScale;
            escalaAtaque.x = transform.localScale.x;
            ataqueCriado.transform.localScale = escalaAtaque;
            Destroy(ataqueCriado, tempoVidaAtaque);
        }

        Collider2D[] inimigosAtingidos = Physics2D.OverlapCircleAll(pontoAtaque.position, raioAtaque, camadaInimigos);

        foreach (Collider2D inimigo in inimigosAtingidos)
        {
            inimigo.SendMessage("TomarDano", danoAtaque, SendMessageOptions.DontRequireReceiver);
        }
    }

    void VerificarVirada()
    {
        if (entradaHorizontal > 0 && !olhandoParaDireita) Virar();
        else if (entradaHorizontal < 0 && olhandoParaDireita) Virar();
    }

    void Virar()
    {
        olhandoParaDireita = !olhandoParaDireita;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    void AjustarGravidade()
    {
        bool segurandoBaixo = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool segurandoPulo = Input.GetButton("Jump") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        if (segurandoBaixo && !estaNoChao)
        {
            rb.gravityScale = gravidadePadrao * multiplicadorQuedaRapida;
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravidadePadrao * multiplicadorQueda;
        }
        else if (rb.linearVelocity.y > 0 && !segurandoPulo)
        {
            rb.gravityScale = gravidadePadrao * multiplicadorPuloCurto;
        }
        else
        {
            rb.gravityScale = gravidadePadrao;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (checadorChao != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(checadorChao.position, tamanhoChecadorChao);
        }

        if (pontoAtaque != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pontoAtaque.position, raioAtaque);
=======
    private float movimentacaoHorizontal;
    private bool estaNoChao;

    private void Start(){
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update(){
        movimentacaoHorizontal = Input.GetAxisRaw("Horizontal");
        if (verificadorDeChao != null){
            estaNoChao = Physics2D.OverlapCircle(verificadorDeChao.position, raioVerificacao, oQueEChao);
        }

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && estaNoChao){
            Pular();
        }
    }

    private void FixedUpdate(){
        rb.linearVelocity = new Vector2(movimentacaoHorizontal * velocidadeMovimento, rb.linearVelocity.y);
    }

    private void Pular(){
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaDoPulo);
    }

    private void OnDrawGizmosSelected(){
        if (verificadorDeChao != null){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(verificadorDeChao.position, raioVerificacao);
>>>>>>> parent of a541cc6 (acho que ficou bom)
        }
    }
}