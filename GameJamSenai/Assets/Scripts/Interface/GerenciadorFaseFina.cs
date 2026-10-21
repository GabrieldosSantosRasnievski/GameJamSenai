using UnityEngine;
using TMPro;
using System.Collections;

public class GerenciadorFaseFinal : MonoBehaviour
{
    public static GerenciadorFaseFinal Instancia;

    [Header("Referências da Câmera")]
    public Camera cameraPrincipal;
    public float zoomOrthographicSize = 3f;
    public float velocidadeZoom = 2f;

    [Header("Interface e Diálogo")]
    public TextMeshProUGUI textoAperteE;
    public GameObject painelVitoria;

    [Header("Entidades")]
    public Transform jogador;
    public ComportamentoFantasma scriptFantasma;

    private bool aguardandoCaptura = false;
    private bool capturado = false;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (textoAperteE != null) textoAperteE.gameObject.SetActive(false);
        if (painelVitoria != null) painelVitoria.SetActive(false);
        if (cameraPrincipal == null) cameraPrincipal = Camera.main;
    }

    public void IniciarSequenciaFinal()
    {
        aguardandoCaptura = true;

        if (textoAperteE != null)
        {
            textoAperteE.text = "Aperte \"E\" para capturar";
            textoAperteE.gameObject.SetActive(true);
        }

        if (scriptFantasma != null)
        {
            scriptFantasma.IniciarInstabilidadeFinal();
        }

        StartCoroutine(RotinaZoomCamera());
    }

    void Update()
    {
        if (aguardandoCaptura && !capturado)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ConcluirCaptura();
            }
        }
    }

    private IEnumerator RotinaZoomCamera()
    {
        while (aguardandoCaptura && cameraPrincipal != null)
        {
            cameraPrincipal.orthographicSize = Mathf.Lerp(
                cameraPrincipal.orthographicSize,
                zoomOrthographicSize,
                velocidadeZoom * Time.deltaTime
            );

            if (jogador != null)
            {
                Vector3 posicaoAlvo = new Vector3(jogador.position.x, jogador.position.y, cameraPrincipal.transform.position.z);
                cameraPrincipal.transform.position = Vector3.Lerp(cameraPrincipal.transform.position, posicaoAlvo, velocidadeZoom * Time.deltaTime);
            }

            yield return null;
        }
    }

    void ConcluirCaptura()
    {
        capturado = true;
        aguardandoCaptura = false;

        if (textoAperteE != null) textoAperteE.gameObject.SetActive(false);
        if (painelVitoria != null) painelVitoria.SetActive(true);
        if (scriptFantasma != null) scriptFantasma.gameObject.SetActive(false);

        Time.timeScale = 0f; // Pausa o jogo ao abrir o painel
    }
}