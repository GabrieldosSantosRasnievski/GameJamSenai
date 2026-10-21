using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ComportamentoFantasma : MonoBehaviour
{
    private bool jaBateu = false;
    public string mensagemAtaque = "O fantasma é imune a seus ataques!";

    public TextMeshProUGUI textoMensagem;
    public float tempoExibicaoMensagem = 3f;

    public Transform jogador;
    private Rigidbody2D rbJogador;
    private VidaJogador vidaJogadorScript;
    private SpriteRenderer spriteRenderer;
    public GameObject prefabProjetil;

    [Header("Efeito de Aparição / Materialização")]
    public float tempoMaterializacao = 1.0f;
    public float alphaInicial = 0.2f;
    private bool seMaterializando = false;

    [Header("Pontos de Disparo por Direção/Fase")]
    public Transform pontoDisparoFase1; // Aponta para a DIREITA
    public Transform pontoDisparoFase2; // Aponta para a ESQUERDA
    public Transform pontoDisparoBaixo; // Aponta para BAIXO (Fase Topo)
    
    [Header("Configurações da Chuva Sem Espaço (Fase Final)")]
    public float limiteArenaEsquerdaX = -8f;
    public float limiteArenaDireitaX = 8f;
    public float espacamentoProjeteis = 1.2f; // Distância entre projéteis da chuva
    public float tempoEntreOndasChuva = 0.8f;
    private bool executandoChuvaFinal = false;

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

    [Header("Mecânica de Sução (Fase 2)")]
    public float tempoParaIniciarSucao = 10f;
    public float duracaoSucao = 10f;
    public float forcaSucaoBase = 150f;
    public float multiplicadorNoAr = 2.5f;
    public float distanciaPertoImpossivel = 3f;
    public float multiplicadorPerto = 3.0f;
    
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

    [Header("Efeito de Luz (Trovoada)")]
    public GameObject painelPreto;

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
    public float velocidadeCorridaVaso = 10f;
    
    private bool estaFugindo = false;
    private bool escondidoNoVaso = false;
    private float temporizadorLuta = 0f;
    private Collider2D colisorFantasma;

    void Awake(){
        colisorFantasma = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start(){
        tempoEntreTiros = tempoTiroFase1;
        
        vasoAtual = vasoDireita;
        scriptVasoAtual = scriptVasoDireita;

        if(textoMensagem != null) textoMensagem.gameObject.SetActive(false);
        if(painelPreto != null) painelPreto.SetActive(false);

        BuscarJogador();
    }

    void BuscarJogador(){
        if(jogador == null){
            GameObject objJogador = GameObject.FindGameObjectWithTag("Player");
            if(objJogador != null) jogador = objJogador.transform;
        }

        if(jogador != null){
            rbJogador = jogador.GetComponent<Rigidbody2D>();
            vidaJogadorScript = jogador.GetComponent<VidaJogador>();
        }
    }

    void OnEnable(){
        escondidoNoVaso = false;
        estaFugindo = false;
        estaSugando = false;
        dandoDash = false;
        executandoChuvaFinal = false;
        temporizadorLuta = 0f;
        temporizadorSucao = 0f;

        posicaoBaseY = transform.position.y;

        // Configuração dos Vasos por Fase
        if(faseAtual == 1){
            tempoEntreTiros = tempoTiroFase1;
            vasoAtual = vasoDireita;
            scriptVasoAtual = scriptVasoDireita;
        }else if(faseAtual == 2){
            tempoEntreTiros = tempoTiroFase2;
            vasoAtual = vasoEsquerda;
            scriptVasoAtual = scriptVasoEsquerda;
        }else if(faseAtual >= 3){
            vasoAtual = vasoTopo;
            scriptVasoAtual = scriptVasoTopo;
        }

        if(colisorFantasma != null) colisorFantasma.enabled = true;

        BuscarJogador();
        StartCoroutine(EfeitoMaterializacao());
    }

    private IEnumerator EfeitoMaterializacao(){
        seMaterializando = true;

        if(spriteRenderer != null){
            Color cor = spriteRenderer.color;
            cor.a = alphaInicial;
            spriteRenderer.color = cor;

            float tempoDecorrido = 0f;

            while(tempoDecorrido < tempoMaterializacao){
                tempoDecorrido += Time.deltaTime;
                cor.a = Mathf.Lerp(alphaInicial, 1.0f, tempoDecorrido / tempoMaterializacao);
                spriteRenderer.color = cor;
                yield return null;
            }

            cor.a = 1.0f;
            spriteRenderer.color = cor;
        }else{
            yield return new WaitForSeconds(tempoMaterializacao);
        }

        seMaterializando = false;
        proximoTiroTempo = Time.time + tempoEntreTiros;
    }

    void Update(){
        if(escondidoNoVaso || dandoDash || seMaterializando || executandoChuvaFinal) return;

        if(!estaFugindo){
            temporizadorLuta += Time.deltaTime;

            // FASE 3: TOPO DA ARENA (Garante deixar o jogador com 1 de vida)
            if(faseAtual >= 3){
                int vidaAtualJogador = GetVidaJogador();

                if(vidaAtualJogador > 1){
                    StartCoroutine(SequenciaChuvaSemEspaco());
                }else{
                    // Jogador já está com 1 de vida -> Trovão final e foge para o vaso topo
                    IniciarFugaParaVaso();
                }
                return;
            }

            // FASE 2: Sução -> Dashes -> Fuga Vaso Esquerda
            if(faseAtual == 2){
                if(temporizadorLuta >= tempoParaIniciarSucao && !estaSugando && temporizadorSucao < duracaoSucao){
                    estaSugando = true;
                }

                if(estaSugando){
                    temporizadorSucao += Time.deltaTime;
                    
                    if(temporizadorSucao >= duracaoSucao){
                        estaSugando = false;
                        StartCoroutine(ExecutarSequenciaDashes());
                        return;
                    }
                }
            }

            // FASE 1: Fuga Vaso Direita após tempo de combate
            if(faseAtual == 1 && temporizadorLuta >= tempoParaTrovoada && vasoAtual != null){
                IniciarFugaParaVaso();
            }else if(!estaSugando){
                AtualizarEixoY();

                if(Time.time >= proximoTiroTempo && prefabProjetil != null && !estaAtirando){
                    StartCoroutine(SequenciaTiro());
                }
            }
        }else{
            MoverParaVaso();
        }
    }

    int GetVidaJogador(){
        if(vidaJogadorScript != null){
            // Usa Reflection leve para acessar vidaAtual sem precisar modificar a visibilidade
            var campoVida = typeof(VidaJogador).GetField("vidaAtual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if(campoVida != null){
                return (int)campoVida.GetValue(vidaJogadorScript);
            }
        }
        return 1;
    }

    private IEnumerator SequenciaChuvaSemEspaco(){
        executandoChuvaFinal = true;

        while(GetVidaJogador() > 1 && !estaFugindo){
            Transform pontoSpawn = (pontoDisparoBaixo != null) ? pontoDisparoBaixo : transform;

            // Instancia projéteis sem nenhum espaço seguro cobrindo a arena
            for(float x = limiteArenaEsquerdaX; x <= limiteArenaDireitaX; x += espacamentoProjeteis){
                Vector3 posicaoDisparo = new Vector3(x, pontoSpawn.position.y, pontoSpawn.position.z);
                Instantiate(prefabProjetil, posicaoDisparo, Quaternion.Euler(0, 0, -90f));
            }

            yield return new WaitForSeconds(tempoEntreOndasChuva);
        }

        executandoChuvaFinal = false;

        // Ao atingir 1 de vida, inicia a trovoada para fugir
        if(!estaFugindo){
            IniciarFugaParaVaso();
        }
    }

    void FixedUpdate(){
        if(estaSugando && !estaFugindo && !escondidoNoVaso && !dandoDash && !seMaterializando && jogador != null && rbJogador != null){
            AplicarForcaSucao();
        }
    }

    void AplicarForcaSucao(){
        Vector2 direcao = (transform.position - jogador.position);
        float distancia = direcao.magnitude;
        Vector2 direcaoNormalizada = direcao.normalized;

        bool noAr = Mathf.Abs(rbJogador.linearVelocity.y) > 0.05f;
        bool segurandoA = Input.GetKey(KeyCode.A);

        if(segurandoA && !noAr){
            rbJogador.linearVelocity = new Vector2(0f, rbJogador.linearVelocity.y);
            return;
        }

        float forcaFinal = forcaSucaoBase;

        if(distancia <= distanciaPertoImpossivel){
            forcaFinal *= multiplicadorPerto;
        }else{
            forcaFinal *= (1f / Mathf.Max(distancia * 0.15f, 0.2f));
        }

        if(noAr){
            forcaFinal *= multiplicadorNoAr;
        }

        rbJogador.AddForce(direcaoNormalizada * forcaFinal, ForceMode2D.Force);
    }

    private IEnumerator ExecutarSequenciaDashes(){
        dandoDash = true;

        float tempoCooldown = 0f;
        while(tempoCooldown < cooldownPosSucao){
            tempoCooldown += Time.deltaTime;
            if(jogador != null){
                posicaoBaseY = Mathf.Lerp(posicaoBaseY, jogador.position.y, velocidadeSeguirY * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, posicaoBaseY, transform.position.z);
            }
            yield return null;
        }

        for(int i = 0; i < quantidadeDashes; i++){
            Transform destinoAtual = (i % 2 == 0) 
                ? ((pontoLimiteEsquerda != null) ? pontoLimiteEsquerda : transform) 
                : ((pontoLimiteDireita != null) ? pontoLimiteDireita : transform);

            if(i > 0){
                float tempoPerseguindo = 0f;
                while(tempoPerseguindo < tempoSeguirPlayer){
                    tempoPerseguindo += Time.deltaTime;
                    if(jogador != null){
                        posicaoBaseY = Mathf.Lerp(posicaoBaseY, jogador.position.y, velocidadeSeguirY * Time.deltaTime);
                        transform.position = new Vector3(transform.position.x, posicaoBaseY, transform.position.z);
                    }
                    yield return null;
                }
            }

            yield return new WaitForSeconds(tempoTravaPreDash);

            while(Vector3.Distance(transform.position, new Vector3(destinoAtual.position.x, transform.position.y, transform.position.z)) > 0.3f){
                float novoX = Mathf.MoveTowards(transform.position.x, destinoAtual.position.x, velocidadeDash * Time.deltaTime);
                transform.position = new Vector3(novoX, transform.position.y, transform.position.z);
                yield return null;
            }
        }

        if(pontoLimiteDireita != null){
            while(Vector3.Distance(transform.position, new Vector3(pontoLimiteDireita.position.x, transform.position.y, transform.position.z)) > 0.3f){
                float novoX = Mathf.MoveTowards(transform.position.x, pontoLimiteDireita.position.x, velocidadeDash * Time.deltaTime);
                transform.position = new Vector3(novoX, transform.position.y, transform.position.z);
                yield return null;
            }
        }

        dandoDash = false;
        IniciarFugaParaVaso();
    }

    void IniciarFugaParaVaso(){
        estaFugindo = true;
        estaSugando = false;
        
        if(colisorFantasma != null) colisorFantasma.enabled = false;
        if(painelPreto != null) StartCoroutine(EfeitoPiscarLuz());
    }

    private IEnumerator EfeitoPiscarLuz(){
        float[] intervalos = new float[] { 0.05f, 0.05f, 0.08f, 0.1f, 0.2f, 0.4f, 0.7f };

        for(int i = 0; i < intervalos.Length; i++){
            painelPreto.SetActive(true);
            yield return new WaitForSeconds(intervalos[i]);
            
            painelPreto.SetActive(false);
            yield return new WaitForSeconds(intervalos[i]);
        }

        painelPreto.SetActive(false);
    }

    void MoverParaVaso(){
        if(vasoAtual == null) return;

        transform.position = Vector3.MoveTowards(transform.position, vasoAtual.position, velocidadeCorridaVaso * Time.deltaTime);

        if(Vector3.Distance(transform.position, vasoAtual.position) < 0.1f){
            EntrarNoVaso();
        }
    }

    void EntrarNoVaso(){
        estaFugindo = false;
        escondidoNoVaso = true;

        if(scriptVasoAtual != null){
            scriptVasoAtual.AtivarVaso();
        }

        if(painelPreto != null) painelPreto.SetActive(false);

        faseAtual++;
        gameObject.SetActive(false);
    }

    void OnDisable(){
        estaSugando = false;
        dandoDash = false;
        seMaterializando = false;
        executandoChuvaFinal = false;
        if(painelPreto != null) painelPreto.SetActive(false);
    }

    void AtualizarEixoY(){
        if(estaAtirando && jogador != null){
            posicaoBaseY = Mathf.Lerp(posicaoBaseY, jogador.position.y, velocidadeSeguirY * Time.deltaTime);
        }

        transform.position = new Vector3(transform.position.x, posicaoBaseY, transform.position.z);
    }

    private IEnumerator SequenciaTiro(){
        estaAtirando = true;

        yield return new WaitForSeconds(0.2f);

        Transform pontoUsado = (faseAtual == 1) ? pontoDisparoFase1 : pontoDisparoFase2;

        if(pontoUsado != null && prefabProjetil != null && !estaFugindo && !estaSugando && !dandoDash && !seMaterializando){
            Instantiate(prefabProjetil, pontoUsado.position, pontoUsado.rotation);
        }

        yield return new WaitForSeconds(0.2f);

        proximoTiroTempo = Time.time + tempoEntreTiros;
        estaAtirando = false;
    }

    private void ProcessarDano(GameObject objetoAtingido){
        if(estaFugindo || escondidoNoVaso || seMaterializando) return;

        if(objetoAtingido.CompareTag("Player")){
            objetoAtingido.SendMessage("TomarDano", danoContato, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerEnter2D(Collider2D outro) => ProcessarDano(outro.gameObject);
    private void OnTriggerStay2D(Collider2D outro) => ProcessarDano(outro.gameObject);
    private void OnCollisionEnter2D(Collision2D colisao) => ProcessarDano(colisao.gameObject);

    public void TomarDano(int dano){
        if(!jaBateu && !estaFugindo && !escondidoNoVaso && !seMaterializando){
            Debug.Log(mensagemAtaque);
            jaBateu = true;
            
            if(textoMensagem != null){
                StartCoroutine(ExibirMensagemTela());
            }
        }
    }

    private IEnumerator ExibirMensagemTela(){
        textoMensagem.text = mensagemAtaque;
        textoMensagem.gameObject.SetActive(true);

        yield return new WaitForSeconds(tempoExibicaoMensagem);

        textoMensagem.gameObject.SetActive(false);
    }
}