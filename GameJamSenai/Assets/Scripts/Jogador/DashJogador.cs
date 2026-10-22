using UnityEngine;
using System.Collections;

public class DashJogador : MonoBehaviour
{
    [Header("Configurações do Dash")]
    [Tooltip("Força/Velocidade do impulso do Dash")]
    public float velocidadeDash = 24f;

    [Tooltip("Duração do Dash em segundos")]
    public float duracaoDash = 0.2f;

    [Tooltip("Tempo de espera (cooldown) até poder usar o Dash novamente")]
    public float cooldownDash = 1.0f;

    [Header("Teclas de Atalho")]
    public KeyCode teclaDashInput = KeyCode.LeftShift;

    private Rigidbody2D rb;
    private Movimento2D movimentoScript;
    private float gravidadeOriginal;

    private bool estaEmDash = false;
    private bool podeDarDash = true;
    private float direcaoX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movimentoScript = GetComponent<Movimento2D>();
    }

    void Update()
    {
        if (estaEmDash) return;

        // Detecta o comando do Dash (Shift ou Botão Direito do Mouse)
        if ((Input.GetKeyDown(teclaDashInput) || Input.GetMouseButtonDown(1)) && podeDarDash)
        {
            StartCoroutine(ExecutarDash());
        }
    }

    void FixedUpdate()
    {
        // Garante a velocidade do Dash fixa a cada frame de física
        if (estaEmDash)
        {
            rb.linearVelocity = new Vector2(direcaoX * velocidadeDash, 0f);
        }
    }

    private IEnumerator ExecutarDash()
    {
        podeDarDash = false;
        estaEmDash = true;

        // 1. Determina a direção do Dash
        direcaoX = Input.GetAxisRaw("Horizontal");
        if (direcaoX == 0f)
        {
            direcaoX = transform.localScale.x > 0 ? 1f : -1f;
        }

        // 2. Salva a gravidade e desativa o script de movimento para não interferir no Dash
        gravidadeOriginal = rb.gravityScale;
        rb.gravityScale = 0f;

        if (movimentoScript != null)
        {
            movimentoScript.enabled = false;
        }

        // 3. Aplica a velocidade inicial do Dash
        rb.linearVelocity = new Vector2(direcaoX * velocidadeDash, 0f);

        // 4. Aguarda a duração do Dash
        yield return new WaitForSeconds(duracaoDash);

        // 5. Restaura a gravidade e reativa o script de movimento
        rb.gravityScale = gravidadeOriginal;
        
        if (movimentoScript != null)
        {
            movimentoScript.enabled = true;
        }

        estaEmDash = false;

        // 6. Cooldown
        yield return new WaitForSeconds(cooldownDash);
        podeDarDash = true;
    }

    public bool EstaEmDash()
    {
        return estaEmDash;
    }
}