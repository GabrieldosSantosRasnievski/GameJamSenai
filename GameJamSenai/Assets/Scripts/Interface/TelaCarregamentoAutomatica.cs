using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TelaCarregamentoAutomatica : MonoBehaviour{
    [SerializeField] private int indiceDaCenaDoJogo = 2;

    private void Start(){
        StartCoroutine(CarregarProximaCenaEmSegundoPlano());
    }
    private IEnumerator CarregarProximaCenaEmSegundoPlano(){
        AsyncOperation operacao = SceneManager.LoadSceneAsync(indiceDaCenaDoJogo);
        operacao.allowSceneActivation = false;
        while (!operacao.isDone){
            if (operacao.progress >= 0.9f){
                operacao.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}