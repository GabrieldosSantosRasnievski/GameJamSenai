using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GerenciamentoMenu : MonoBehaviour{
    public void IrParaTelaDeCarregamento(int indiceDaCena){
        StartCoroutine(TrocarDeCenaAssincrono(indiceDaCena));
    }

    private IEnumerator TrocarDeCenaAssincrono(int indiceDaCena){
        AsyncOperation operacao = SceneManager.LoadSceneAsync(indiceDaCena);
        while (!operacao.isDone){
            yield return null;
        }
    }
}