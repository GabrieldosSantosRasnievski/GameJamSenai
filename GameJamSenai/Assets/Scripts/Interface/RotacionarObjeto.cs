using UnityEngine;

public class RotacionarObjeto : MonoBehaviour{
    [SerializeField] private Vector3 velocidadeDeRotacao = new Vector3(0f, 0f, -200f);

    private void Update(){
        transform.Rotate(velocidadeDeRotacao * Time.deltaTime);
    }
}