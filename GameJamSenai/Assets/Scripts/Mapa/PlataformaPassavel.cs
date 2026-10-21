using UnityEngine;
using System.Collections;

public class PlataformaPassavel : MonoBehaviour
{
    private PlatformEffector2D effector;
    public float tempoDesativado = 0.3f;

    void Start(){
        effector = GetComponent<PlatformEffector2D>();
    }

    void Update(){
        bool segurandoBaixo = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool apertouPulo = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);

        if(segurandoBaixo && apertouPulo){
            StartCoroutine(DesativarColisao());
        }
    }

    private IEnumerator DesativarColisao(){
        effector.rotationalOffset = 180f;
        yield return new WaitForSeconds(tempoDesativado);
        effector.rotationalOffset = 0f;
    }
}