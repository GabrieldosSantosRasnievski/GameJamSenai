using UnityEngine;
using System.Collections;

public class ProjetilFantasma : MonoBehaviour
{
    public float velocidade = 8f;
    public float tempoVida = 5f;
    public int dano = 1;

    private Vector3 direcaoMovimento = Vector3.right;

    void Start(){
        // Busca o fantasma na cena para saber em qual fase o jogo está
        ComportamentoFantasma fantasma = FindObjectOfType<ComportamentoFantasma>();

        if(fantasma != null){
            if(fantasma.faseAtual == 1){
                direcaoMovimento = Vector3.right; // Direita na Fase 1
            }else if(fantasma.faseAtual == 2){
                direcaoMovimento = Vector3.left;  // Esquerda na Fase 2
            }else if(fantasma.faseAtual >= 3){
                direcaoMovimento = Vector3.down;  // BAIXO na Fase Final (Topo)
            }
        }else{
            // Fallback: se não achar o Boss, move na direção em que o ponto de disparo está apontando
            direcaoMovimento = transform.right; 
        }

        StartCoroutine(MoverEDestruir());
    }

    private IEnumerator MoverEDestruir(){
        float tempoDecorrido = 0f;

        while(tempoDecorrido < tempoVida){
            // Move no espaço do mundo na direção definida pela fase
            transform.Translate(direcaoMovimento * velocidade * Time.deltaTime, Space.World);
            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D outro){
        if(outro.CompareTag("Player")){
            outro.SendMessage("TomarDano", dano, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}