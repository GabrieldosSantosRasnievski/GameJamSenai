using UnityEngine;

public class Movimentacao2D : MonoBehaviour
{
    [SerializeField] private float velocidadeMovimento = 8f;
    [SerializeField] private float forcaDoPulo = 12f;
    [SerializeField] private Transform verificadorDeChao;
    [SerializeField] private LayerMask oQueEChao;
    [SerializeField] private float raioVerificacao = 0.2f;
    private Rigidbody2D rb;
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
        }
    }
}