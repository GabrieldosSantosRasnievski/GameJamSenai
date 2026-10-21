using UnityEngine;

public class Movimento2D : MonoBehaviour
{
    private Rigidbody2D rb;
    public float velocidadeMovimento = 8f;
    private float entradaHorizontal;
    private bool olhandoParaDireita = true;

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

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravidadePadrao;
    }

    void Update(){
        entradaHorizontal = Input.GetAxisRaw("Horizontal");

        VerificarVirada();

        if(checadorChao != null){
            estaNoChao = Physics2D.OverlapBox(checadorChao.position, tamanhoChecadorChao, 0f, camadaChao);
        }

        if(estaNoChao){
            coyoteTempoContador = tempoCoyote;
        }else{
            coyoteTempoContador -= Time.deltaTime;
        }

        bool segurandoBaixo = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool apertouPulo = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

        if(apertouPulo && coyoteTempoContador > 0f && !segurandoBaixo){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, forcaPulo);
            coyoteTempoContador = 0f;
        }

        AjustarGravidade();

        if(Time.time >= proximoAtaqueTempo){
            bool apertouAtaque = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J);

            if(apertouAtaque){
                Atacar();
                proximoAtaqueTempo = Time.time + tempoEntreAtaques;
            }
        }
    }

    void FixedUpdate(){
        rb.linearVelocity = new Vector2(entradaHorizontal * velocidadeMovimento, rb.linearVelocity.y);
    }

    void Atacar(){
        if(pontoAtaque == null){
            return;
        }

        if(prefabAtaque != null){
            GameObject ataqueCriado = Instantiate(prefabAtaque, pontoAtaque.position, transform.rotation);
            Vector3 escalaAtaque = ataqueCriado.transform.localScale;
            escalaAtaque.x = transform.localScale.x;
            ataqueCriado.transform.localScale = escalaAtaque;
            Destroy(ataqueCriado, tempoVidaAtaque);
        }

        Collider2D[] inimigosAtingidos = Physics2D.OverlapCircleAll(pontoAtaque.position, raioAtaque, camadaInimigos);

        foreach(Collider2D inimigo in inimigosAtingidos){
            inimigo.SendMessage("TomarDano", danoAtaque, SendMessageOptions.DontRequireReceiver);
        }
    }

    void VerificarVirada(){
        if(entradaHorizontal > 0 && !olhandoParaDireita){
            Virar();
        }else if(entradaHorizontal < 0 && olhandoParaDireita){
            Virar();
        }
    }

    void Virar(){
        olhandoParaDireita = !olhandoParaDireita;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    void AjustarGravidade(){
        bool segurandoBaixo = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool segurandoPulo = Input.GetButton("Jump") || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        if(segurandoBaixo && !estaNoChao){
            rb.gravityScale = gravidadePadrao * multiplicadorQuedaRapida;
        }else if(rb.linearVelocity.y < 0){
            rb.gravityScale = gravidadePadrao * multiplicadorQueda;
        }else if(rb.linearVelocity.y > 0 && !segurandoPulo){
            rb.gravityScale = gravidadePadrao * multiplicadorPuloCurto;
        }else{
            rb.gravityScale = gravidadePadrao;
        }
    }

    private void OnDrawGizmosSelected(){
        if(checadorChao != null){
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(checadorChao.position, tamanhoChecadorChao);
        }

        if(pontoAtaque != null){
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(pontoAtaque.position, raioAtaque);
        }
    }
}