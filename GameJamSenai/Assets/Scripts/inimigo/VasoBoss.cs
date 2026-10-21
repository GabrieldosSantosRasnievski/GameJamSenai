using UnityEngine;

public class VasoBoss : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Collider2D colisor;
    public Color corAtivo = Color.blue;
    public bool vulneravel = false;

    [Header("Visual do Vaso Espatifado")]
    public Sprite spriteEspatifado;

    [Header("Conexão com o Fantasma")]
    public GameObject fantasma;
    public Transform pontoRessurgirDireita; // Posição para onde o fantasma vai ao sair do vaso

    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();
        colisor = GetComponent<Collider2D>();

        if(colisor != null){
            colisor.enabled = false;
        }
    }

    public void AtivarVaso(){
        vulneravel = true;

        if(spriteRenderer != null){
            spriteRenderer.color = corAtivo;
        }

        if(colisor != null){
            colisor.enabled = true;
        }
    }

    public void TomarDano(int dano){
        if(vulneravel){
            Debug.Log("Vaso quebrado! O fantasma saiu!");
            
            vulneravel = false;

            // Troca a arte pelo vaso espatifado e limpa a cor azul
            if(spriteRenderer != null){
                if(spriteEspatifado != null){
                    spriteRenderer.sprite = spriteEspatifado;
                }
                spriteRenderer.color = Color.white; // Reseta a cor para exibir o sprite limpo
            }

            // Desativa a colisão definitivamente
            if(colisor != null){
                colisor.enabled = false;
            }

            // Reativa o fantasma na posição da direita
            if(fantasma != null){
                if(pontoRessurgirDireita != null){
                    fantasma.transform.position = pontoRessurgirDireita.position;
                }
                
                fantasma.SetActive(true);
            }
        }
    }
}