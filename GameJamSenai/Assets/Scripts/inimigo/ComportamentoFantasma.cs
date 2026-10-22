using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ComportamentoFantasma : MonoBehaviour
{
    private bool jaBateu = false;
    public string mensagemAtaque = "O fantasma é imune a seus ataques!";

    public TextMeshProUGUI textoMensagem;
    public float tempoExibicaoMensagem = 3f;

    public Transform jogador;
    private Rigidbody2D rbJogador;
    private VidaJogador vidaJogadorScript;
    private Movimento2D movimentoJogadorScript;
    private SpriteRenderer spriteRenderer;
    public GameObject prefabProjetil;

    [Header("Controlador do Trovão Externo")]
    public EfeitoTrovao controladorTrovao;

    [Header("Pausas de Transição (Apenas Tiro->Sução e Sução->Dash)")]
    public float intervaloEntreAcoes = 1.5f;
    private bool aguardandoIntervalo = false;

    [Header("Indicador de Zona Segura")]
    public GameObject prefabZonaSegura;
    public float alturaChaoY = -4f;
    public float alturaColunaZonaSegura = 10f;

    [Header("Efeito de Aparição / Materialização")]
    public float tempoMaterializacao = 1.0f;
    public float alphaInicial = 0.2f;
    private bool seMaterializando = false;

    [Header("Pontos de Disparo por Direção/Fase")]
    public Transform pontoDisparoFase1;
    public Transform pontoDisparoFase2;
    public Transform pontoDisparoBaixo;

    [Header("Configurações da Chuva Sem Espaço (Fase Final)")]
    public float limiteArenaEsquerdaX = -8f;
    public float limiteArenaDireitaX = 8f;
    public float espacamentoProjeteis = 1.2f;
    public float tempoEntreOndasChuva = 0.8f;
    public int deslocamentoMaximoZonaSegura = 3;
    public int maxOndasChuva = 10;
    private int contadorOndasChuva = 0;
    private bool executandoChuvaFinal = false;
    private bool chuvaConcluida = false;
    private int ultimoIndiceZonaSegura = -1;

    [Header("Cadência por Fase")]
    public float tempoTiroFase1 = 1.0f;
    public float tempoTiroFase2 = 0.5f;
    private float tempoEntreTiros;
    private float proximoTiroTempo;

    public float velocidadeSeguirY = 6f;
    public int danoContato = 1;

    private float posicaoBaseY;
    private bool estaAtirando = false;

    [Header("Controle de Fase do Boss")]
    public int faseAtual = 1;

    [Header("Mecânica de Sução Ajustada")]
    public Transform pontoSucao;
    public float velocidadeIdaPontoSucao = 12f;
    public float tempoParaIniciarSucao = 10f;
    public float duracaoSucao = 10f;
    public float forcaSucaoBase = 220f;
    [Tooltip("Percentual mantido ao segurar A: forte o bastante para arrastá-lo para trás devagar")]
    public float multiplicadorAoResistirA = 0.65f; 
    [Tooltip("Percentual da força quando pula: fraco o bastante para o pulo permitir escapar")]
    public float multiplicadorPuloFuga = 0.1f; 

    private bool estaSugando = false;
    private float temporizadorSucao = 0f;

    [Header("Mecânica de Dash (Fase 2)")]
    public Transform pontoLimiteEsquerda;
    public Transform pontoLimiteDireita;
    public float velocidadeDash = 35f;
    public int quantidadeDashes = 3;
    public float cooldownPosSucao = 2.0f;
    public float tempoSeguirPlayer = 2.0f;
    public float tempoTravaPreDash = 0.5f;
    private bool dandoDash = false;

    [Header("Instabilidade Final")]
    public float tempoEntreTeleportes = 0.15f;
    private bool estaInstavel = false;

    [Header("Efeito de Luz (Trovoada)")]
    public GameObject painelPreto;
    public float alphaInstabilidade = 0.5f;

    [Header("Mecânica dos 3 Vasos")]
    public Transform vasoDireita;
    public VasoBoss scriptVasoDireita;

    public Transform vasoEsquerda;
    public VasoBoss scriptVasoEsquerda;

    public Transform vasoTopo;
    public VasoBoss scriptVasoTopo;

    public Transform vasoAtual;
    public VasoBoss scriptVasoAtual;
    public float tempoParaTrovoada = 30f;
    [Tooltip("Velocidade do fantasma ao caminhar até o vaso (valores menores deixam a ida mais lenta)")]
    public float velocidadeCorridaVaso = 3f;

    private bool estaFugindo = false;
    private bool escondidoNoVaso = false;
    private float temporizadorLuta = 0f;
    private Collider2D colisorFantasma;

    void Awake()
    {
        colisorFantasma = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        tempoEntreTiros = tempoTiroFase1;

        vasoAtual = vasoDireita;
        scriptVasoAtual = scriptVasoDireita;

        if (textoMensagem != null) textoMensagem.gameObject.SetActive(false);
        if (painelPreto != null) painelPreto.SetActive(false);

        BuscarJogador();
    }

    void BuscarJogador()
    {
        if (jogador == null)
        {
            GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
            if (objJogador != null) jogador = objJogador.transform;
        }

        if (jogador != null)
        {
            rbJogador = jogador.GetComponent<Rigidbody2D>();
            vidaJogadorScript = jogador.GetComponent<VidaJogador>();
            movimentoJogadorScript = jogador.GetComponent<Movimento2D>();
        }
    }

    void OnEnable()
    {
        escondidoNoVaso = false;
        estaFugindo = false;
        estaSugando = false;
        dandoDash = false;
        executandoChuvaFinal = false;
        chuvaConcluida = false;
        estaInstavel = false;
        aguardandoIntervalo = false;
        temporizadorLuta = 0f;
        temporizadorSucao = 0f;
        ultimoIndiceZonaSegura = -1;
        contadorOndasChuva = 0;

        posicaoBaseY = transform.position.y;

        AtualizarVasoDaFase();

        if (colisorFantasma != null) colisorFantasma.enabled = true;

        BuscarJogador();
        StartCoroutine(EfeitoMaterializacao());
    }

    void AtualizarVasoDaFase()
    {
        if (faseAtual == 1)
        {
            tempoEntreTiros = tempoTiroFase1;
            vasoAtual = vasoDireita;
            scriptVasoAtual = scriptVasoDireita;
        }
        else if (faseAtual == 2)
        {
            tempoEntreTiros = tempoTiroFase2;
            vasoAtual = vasoEsquerda;
            scriptVasoAtual = scriptVasoEsquerda;
        }
        else if (faseAtual >= 3)
        {
            vasoAtual = vasoTopo;
            scriptVasoAtual = scriptVasoTopo;
        }
    }

    private IEnumerator EfeitoMaterializacao()
    {
        seMaterializando = true;

        if (spriteRenderer != null)
        {
            Color cor = spriteRenderer.color;
            cor.a = alphaInicial;
            spriteRenderer.color = cor;

            float tempoDecorrido = 0f;

            while (tempoDecorrido < tempoMaterializacao)
            {
                tempoDecorrido += Time.deltaTime;
                cor.a = Mathf.Lerp(alphaInicial, 1.0f, tempoDecorrido / tempoMaterializacao);
                spriteRenderer.color = cor;
                yield return null;
            }

            cor.a = 1.0f;
            spriteRenderer.color = cor;
        }
        else
        {
            yield return new WaitForSeconds(tempoMaterializacao);
        }

        seMaterializando = false;
        proximoTiroTempo = Time.time + tempoEntreTiros;
    }

    void Update()
    {
        if (escondidoNoVaso || dandoDash || seMaterializando || executandoChuvaFinal || estaInstavel || aguardandoIntervalo) return;

        if (!estaFugindo)
        {
            temporizadorLuta += Time.deltaTime;

            if (faseAtual >= 3)
            {
                if (!chuvaConcluida)
                {
                    StartCoroutine(SequenciaChuvaSemEspaco());
                }
                return;
            }

            if (faseAtual == 2)
            {
                if (temporizadorLuta >= tempoParaIniciarSucao && !estaSugando && temporizadorSucao < duracaoSucao)
                {
                    StartCoroutine(RotinaTransicaoParaSucao());
                    return;
                }

                if (estaSugando)
                {
                    temporizadorSucao += Time.deltaTime;

                    if (pontoSucao != null)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, pontoSucao.position, velocidadeIdaPontoSucao * Time.deltaTime);
                    }

                    if (temporizadorSucao >= duracaoSucao)
                    {
                        PararSucao();
                        StartCoroutine(RotinaTransicaoParaDash());
                        return;
                    }
                }
            }

            if (faseAtual == 1 && temporizadorLuta >= tempoParaTrovoada && vasoAtual != null)
            {
                IniciarFugaParaVaso();
            }
            else if (!estaSugando)
            {
                AtualizarEixoY();

                if (Time.time >= proximoTiroTempo && prefabProjetil != null && !estaAtirando)
                {
                    StartCoroutine(SequenciaTiro());
                }
            }
        }
        else
        {
            MoverParaVaso();
        }
    }

    private IEnumerator RotinaTransicaoParaSucao()
    {
        aguardandoIntervalo = true;
        yield return new WaitForSeconds(intervaloEntreAcoes);
        aguardandoIntervalo = false;

        estaSugando = true;
    }

    private IEnumerator RotinaTransicaoParaDash()
    {
        aguardandoIntervalo = true;
        yield return new WaitForSeconds(intervaloEntreAcoes);
        aguardandoIntervalo = false;

        StartCoroutine(ExecutarSequenciaDashes());
    }

    void FixedUpdate()
    {
        if (estaSugando && !estaFugindo && !escondidoNoVaso && !dandoDash && !seMaterializando && !estaInstavel && jogador != null && rbJogador != null)
        {
            AplicarForcaSucao();
        }
    }

    void AplicarForcaSucao()
    {
        if (ChecarJogadorInvencivel())
        {
            if (movimentoJogadorScript != null) movimentoJogadorScript.SetSendoSugado(false);
            return;
        }

        if (movimentoJogadorScript != null) movimentoJogadorScript.SetSendoSugado(true);

        Vector2 direcao = (transform.position - jogador.position).normalized;

        bool noAr = Mathf.Abs(rbJogador.linearVelocity.y) > 0.05f;
        bool segurandoA = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);

        float forcaFinal = forcaSucaoBase;

        if (noAr)
        {
            forcaFinal *= multiplicadorPuloFuga;
        }
        else if (segurandoA)
        {
            forcaFinal *= multiplicadorAoResistirA;
        }

        rbJogador.AddForce(direcao * forcaFinal, ForceMode2D.Force);
    }

    void PararSucao()
    {
        estaSugando = false;
        if (movimentoJogadorScript != null)
        {
            movimentoJogadorScript.SetSendoSugado(false);
        }
    }

    bool ChecarJogadorInvencivel()
    {
        if (vidaJogadorScript == null) return false;

        var campoInvencivel = typeof(VidaJogador).GetField("invencivel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                           ?? typeof(VidaJogador).GetField("estaInvencivel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (campoInvencivel != null)
        {
            return (bool)campoInvencivel.GetValue(vidaJogadorScript);
        }

        return false;
    }

    private IEnumerator SequenciaChuvaSemEspaco()
    {
        executandoChuvaFinal = true;

        Transform pontoSpawn = (pontoDisparoBaixo != null) ? pontoDisparoBaixo : transform;

        List<float> posicoesX = new List<float>();
        for (float x = limiteArenaEsquerdaX; x <= limiteArenaDireitaX; x += espacamentoProjeteis)
        {
            posicoesX.Add(x);
        }

        if (posicoesX.Count >= 2)
        {
            int maxIndiceValido = posicoesX.Count - 2;
            int indiceEspacoSeguro;

            if (ultimoIndiceZonaSegura < 0 || ultimoIndiceZonaSegura > maxIndiceValido)
            {
                indiceEspacoSeguro = Random.Range(0, maxIndiceValido + 1);
            }
            else
            {
                int minRange = Mathf.Max(0, ultimoIndiceZonaSegura - deslocamentoMaximoZonaSegura);
                int maxRange = Mathf.Min(maxIndiceValido, ultimoIndiceZonaSegura + deslocamentoMaximoZonaSegura);

                indiceEspacoSeguro = Random.Range(minRange, maxRange + 1);
            }

            ultimoIndiceZonaSegura = indiceEspacoSeguro;

            for (int i = 0; i < posicoesX.Count; i++)
            {
                if (i == indiceEspacoSeguro || i == indiceEspacoSeguro + 1)
                {
                    continue;
                }

                Vector3 posicaoDisparo = new Vector3(posicoesX[i], pontoSpawn.position.y, pontoSpawn.position.z);
                Instantiate(prefabProjetil, posicaoDisparo, Quaternion.identity);
            }

            float posXCentroSeguro = (posicoesX[indiceEspacoSeguro] + posicoesX[indiceEspacoSeguro + 1]) / 2f;
            Vector3 posicaoIndicador = new Vector3(posXCentroSeguro, alturaChaoY, 0f);

            if (prefabZonaSegura != null)
            {
                GameObject indicador = Instantiate(prefabZonaSegura, posicaoIndicador, Quaternion.identity);
                ConfigurarGradienteVerde(indicador);
                Destroy(indicador, tempoEntreOndasChuva);
            }
        }

        contadorOndasChuva++;

        yield return new WaitForSeconds(tempoEntreOndasChuva);

        executandoChuvaFinal = false;

        if (contadorOndasChuva >= maxOndasChuva)
        {
            chuvaConcluida = true;
            AtualizarVasoDaFase();
            IniciarFugaParaVaso();
        }
    }

    void ConfigurarGradienteVerde(GameObject objetoIndicador)
    {
        LineRenderer lr = objetoIndicador.GetComponent<LineRenderer>();
        if (lr == null) lr = objetoIndicador.AddComponent<LineRenderer>();

        lr.sortingOrder = 20;
        lr.useWorldSpace = false;

        lr.SetPosition(0, Vector3.zero);
        lr.SetPosition(1, new Vector3(0f, alturaColunaZonaSegura, 0f));

        Gradient gradiente = new Gradient();

        GradientColorKey[] chavesCor = new GradientColorKey[2];
        chavesCor[0] = new GradientColorKey(new Color(0f, 1f, 0.2f), 0.0f);
        chavesCor[1] = new GradientColorKey(new Color(0.6f, 0.9f, 0.6f), 1.0f);

        GradientAlphaKey[] chavesAlpha = new GradientAlphaKey[2];
        chavesAlpha[0] = new GradientAlphaKey(0.85f, 0.0f);
        chavesAlpha[1] = new GradientAlphaKey(0.0f, 1.0f);

        gradiente.SetKeys(chavesCor, chavesAlpha);
        lr.colorGradient = gradiente;
    }

    public void IniciarInstabilidadeFinal()
    {
        estaInstavel = true;
        PararSucao();
        dandoDash = false;
        estaFugindo = false;
        executandoChuvaFinal = false;

        if (colisorFantasma != null) colisorFantasma.enabled = false;
        StartCoroutine(RotinaInstabilidade());
    }

    private IEnumerator RotinaInstabilidade()
    {
        if (painelPreto != null)
        {
            painelPreto.SetActive(true);
            DefinirAlphaPainelPreto(alphaInstabilidade);
        }

        while (estaInstavel)
        {
            float minX = (pontoLimiteEsquerda != null) ? pontoLimiteEsquerda.position.x : transform.position.x - 5f;
            float maxX = (pontoLimiteDireita != null) ? pontoLimiteDireita.position.x : transform.position.x + 5f;

            float novoX = Random.Range(minX, maxX);
            float novoY = Random.Range(posicaoBaseY - 2f, posicaoBaseY + 3f);
            transform.position = new Vector3(novoX, novoY, transform.position.z);

            if (spriteRenderer != null)
            {
                Color cor = spriteRenderer.color;
                cor.a = Random.Range(0.2f, 0.85f);
                spriteRenderer.color = cor;
            }

            if (painelPreto != null)
            {
                bool visivel = Random.value > 0.3f;
                painelPreto.SetActive(visivel);
            }

            yield return new WaitForSeconds(tempoEntreTeleportes);
        }

        RestaurarPainelPreto();
    }

    private void DefinirAlphaPainelPreto(float alpha)
    {
        if (painelPreto != null)
        {
            Image img = painelPreto.GetComponent<Image>();
            if (img != null)
            {
                Color cor = img.color;
                cor.a = alpha;
                img.color = cor;
            }
        }
    }

    private void RestaurarPainelPreto()
    {
        DefinirAlphaPainelPreto(1.0f);
        if (painelPreto != null) painelPreto.SetActive(false);
    }

    private IEnumerator ExecutarSequenciaDashes()
    {
        dandoDash = true;

        float tempoCooldown = 0f;
        while (tempoCooldown < cooldownPosSucao)
        {
            tempoCooldown += Time.deltaTime;
            if (jogador != null)
            {
                posicaoBaseY = Mathf.Lerp(posicaoBaseY, jogador.position.y, velocidadeSeguirY * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, posicaoBaseY, transform.position.z);
            }
            yield return null;
        }

        for (int i = 0; i < quantidadeDashes; i++)
        {
            Transform destinoAtual = (i % 2 == 0)
                ? ((pontoLimiteEsquerda != null) ? pontoLimiteEsquerda : transform)
                : ((pontoLimiteDireita != null) ? pontoLimiteDireita : transform);

            if (i > 0)
            {
                float tempoPerseguindo = 0f;
                while (tempoPerseguindo < tempoSeguirPlayer)
                {
                    tempoPerseguindo += Time.deltaTime;
                    if (jogador != null)
                    {
                        posicaoBaseY = Mathf.Lerp(posicaoBaseY, jogador.position.y, velocidadeSeguirY * Time.deltaTime);
                        transform.position = new Vector3(transform.position.x, posicaoBaseY, transform.position.z);
                    }
                    yield return null;
                }
            }

            yield return new WaitForSeconds(tempoTravaPreDash);

            while (Vector3.Distance(transform.position, new Vector3(destinoAtual.position.x, transform.position.y, transform.position.z)) > 0.3f)
            {
                float novoX = Mathf.MoveTowards(transform.position.x, destinoAtual.position.x, velocidadeDash * Time.deltaTime);
                transform.position = new Vector3(novoX, transform.position.y, transform.position.z);
                yield return null;
            }
        }

        if (pontoLimiteDireita != null)
        {
            while (Vector3.Distance(transform.position, new Vector3(pontoLimiteDireita.position.x, transform.position.y, transform.position.z)) > 0.3f)
            {
                float novoX = Mathf.MoveTowards(transform.position.x, pontoLimiteDireita.position.x, velocidadeDash * Time.deltaTime);
                transform.position = new Vector3(novoX, transform.position.y, transform.position.z);
                yield return null;
            }
        }

        dandoDash = false;
        IniciarFugaParaVaso();
    }

    void IniciarFugaParaVaso()
    {
        estaFugindo = true;
        PararSucao();

        if (colisorFantasma != null) colisorFantasma.enabled = false;

        if (controladorTrovao != null)
        {
            controladorTrovao.DispararTrovao();
        }
    }

    void MoverParaVaso()
    {
        if (vasoAtual == null) return;

        transform.position = Vector3.MoveTowards(transform.position, vasoAtual.position, velocidadeCorridaVaso * Time.deltaTime);

        if (Vector3.Distance(transform.position, vasoAtual.position) < 0.1f)
        {
            EntrarNoVaso();
        }
    }

    void EntrarNoVaso()
    {
        estaFugindo = false;
        escondidoNoVaso = true;

        if (scriptVasoAtual != null)
        {
            scriptVasoAtual.AtivarVaso();
        }

        faseAtual++;
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        PararSucao();
        dandoDash = false;
        seMaterializando = false;
        executandoChuvaFinal = false;
        estaInstavel = false;
        aguardandoIntervalo = false;
    }

    void AtualizarEixoY()
    {
        if (estaAtirando && jogador != null)
        {
            posicaoBaseY = Mathf.Lerp(posicaoBaseY, jogador.position.y, velocidadeSeguirY * Time.deltaTime);
        }

        transform.position = new Vector3(transform.position.x, posicaoBaseY, transform.position.z);
    }

    private IEnumerator SequenciaTiro()
    {
        estaAtirando = true;

        yield return new WaitForSeconds(0.2f);

        Transform pontoUsado = (faseAtual == 1) ? pontoDisparoFase1 : pontoDisparoFase2;

        if (pontoUsado != null && prefabProjetil != null && !estaFugindo && !estaSugando && !dandoDash && !seMaterializando && !estaInstavel)
        {
            Instantiate(prefabProjetil, pontoUsado.position, pontoUsado.rotation);
        }

        yield return new WaitForSeconds(0.2f);

        proximoTiroTempo = Time.time + tempoEntreTiros;
        estaAtirando = false;
    }

    private void ProcessarDano(GameObject objetoAtingido)
    {
        if (estaFugindo || escondidoNoVaso || seMaterializando || estaInstavel) return;

        if (objetoAtingido.CompareTag("Player"))
        {
            objetoAtingido.SendMessage("TomarDano", danoContato, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerEnter2D(Collider2D outro) => ProcessarDano(outro.gameObject);
    private void OnTriggerStay2D(Collider2D outro) => ProcessarDano(outro.gameObject);
    private void OnCollisionEnter2D(Collision2D colisao) => ProcessarDano(colisao.gameObject);

    public void TomarDano(int dano)
    {
        if (!jaBateu && !estaFugindo && !escondidoNoVaso && !seMaterializando && !estaInstavel)
        {
            Debug.Log(mensagemAtaque);
            jaBateu = true;

            if (textoMensagem != null)
            {
                StartCoroutine(ExibirMensagemTela());
            }
        }
    }

    private IEnumerator ExibirMensagemTela()
    {
        textoMensagem.text = mensagemAtaque;
        textoMensagem.gameObject.SetActive(true);

        yield return new WaitForSeconds(tempoExibicaoMensagem);

        textoMensagem.gameObject.SetActive(false);
    }
}