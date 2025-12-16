using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BossAI : MonoBehaviour
{
    // --- Header Umum ---
    [Header("Status Umum")]
    public float health = 2000f;
    public float maxHealth = 2000f;
    public int totalDamageDealt = 0;
    public string currentAIModeName; // Untuk debug di Inspector

    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    // --- Pengaturan Gerakan ---
    [Header("Pengaturan Gerakan")]
    [SerializeField] private float moveSpeed = 8f;
    
    // --- Referensi Prefab & Transform ---
    [Header("Referensi Aset")]
    [SerializeField] private GameObject groundSlamPrefab;
    [SerializeField] private Transform groundSlamPoint;
    [SerializeField] private GameObject laserBeamPrefab;
    [SerializeField] private Transform laserFirePoint;
    [SerializeField] private GameObject shieldVisualPrefab; // PASTIKAN SUDAH DI-DRAG
    [SerializeField] private Transform shieldSpawnPoint;   // PASTIKAN SUDAH DI-DRAG

    // --- ANIMATION REFERENCE ---
    private Animator anim; 

    // --- Pengaturan Shield ---
    [Header("Pengaturan Shield")]
    [SerializeField] private float shieldDuration = 4f;   
    [SerializeField] private float shieldCooldown = 20f;  

    // --- Parameter SET B ---
    [Header("Parameter Khusus FuSM Set B")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaCostChase = 10f;
    [SerializeField] private float staminaCostSlam = 30f;
    [SerializeField] private float staminaCostLaser = 40f;
    
    [SerializeField] private float rechargeThreshold = 50f;

    // --- Internal Variables ---
    private Transform currentTarget;
    private Rigidbody2D rb;
    private float thinkTimer;
    private AIMode currentMode;
    
    // State variables
    private float currentStamina;
    private bool isInvincible = false;
    private bool isOnGround = true;
    private string currentAction = "IDLE";
    private bool isFacingRight = true; 
    private bool isRecharging = false;
    
    // Cooldowns
    private float shieldCooldownTimer = 0f;
    private float repositionCooldownTimer = 0f;
    
    // FSM Specifics (Mode 1)
    private bool fsmHasShieldedOnce = false; 
    private float fsmRepositionTimer = 0f;   

    [Header("Pengaturan Reposition (Lompat)")]
    [SerializeField] private float repositionJumpForce = 10f; 
    [SerializeField] private float repositionCooldown = 8f;

    [Header("AI Settings - Interval (Set B)")]
    [Tooltip("Interval paling lambat (saat Sehat)")]
    [SerializeField] private float maxThinkInterval = 1.5f; 
    [Tooltip("Interval paling cepat (saat Kritis 100%)")]
    [SerializeField] private float minThinkInterval = 0.7f; 
    
    [SerializeField] private float aiUrgencyThreshold = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // --- INIT ANIMATOR ---
        anim = GetComponent<Animator>();
        
        // MENGAMBIL MODE DARI GAMESETTINGS
        currentMode = GameSettings.SelectedMode;
        currentAIModeName = currentMode.ToString();

        currentTarget = player1;
        currentStamina = maxStamina;
        
        if (currentMode == AIMode.FuSM_SetB) thinkTimer = maxThinkInterval; 
        else thinkTimer = 1.5f; 
    }

    void Update()
    {
        if (health <= 0) return;

        // 1. Timer Berpikir
        thinkTimer -= Time.deltaTime;
        if (thinkTimer <= 0f)
        {
            switch (currentMode)
            {
                case AIMode.FSM_Conventional:
                    Think_FSM();
                    break;
                case AIMode.FuSM_SetA:
                    Think_FuSM_SetA();
                    break;
                case AIMode.FuSM_SetB:
                    Think_FuSM_SetB();
                    break;
            }
        }

        // 2. Update Timer & Cooldowns
        UpdateTimers();

        // 3. Manajemen Stamina (Hanya Set B)
        if (currentMode == AIMode.FuSM_SetB)
        {
            UpdateStaminaLogic();
        }

        // 4. Logika Flip (Semua Mode)
        if (currentTarget != null)
        {
            if (currentTarget.position.x > transform.position.x && isFacingRight)
            {
                Flip();
            }
            else if (currentTarget.position.x < transform.position.x && !isFacingRight)
            {
                Flip();
            }
        }

        // 5. Update Animasi
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        if (anim == null) return;

        bool isRunning = (currentAction == "CHASE_PLAYER");
        anim.SetBool("Run", isRunning);

        bool isIdle = (currentAction == "IDLE" || currentAction == "SHIELD");
        anim.SetBool("Idle", isIdle);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    // --- LOGIKA THINKING (FSM/FuSM) ---
    private void Think_FSM()
    {
        if (!CheckTargets()) return;
        
        float distP1 = (player1 != null) ? Vector2.Distance(transform.position, player1.position) : Mathf.Infinity;
        float distP2 = (player2 != null) ? Vector2.Distance(transform.position, player2.position) : Mathf.Infinity;
        currentTarget = (distP1 < distP2) ? player1 : player2;
        float distance = Mathf.Min(distP1, distP2);
        float hpPercent = health / maxHealth;

        string actionToTake = "CHASE_PLAYER"; 

        if (hpPercent < 0.2f && !fsmHasShieldedOnce) 
        {
            actionToTake = "SHIELD";
            fsmHasShieldedOnce = true;
        }
        else if (fsmRepositionTimer >= 10f) 
        {
            actionToTake = "REPOSITION";
            fsmRepositionTimer = 0f; 
        }
        else if (distance < 5f)
        {
            actionToTake = "GROUND_SLAM";
        }
        else if (distance > 15f)
        {
            actionToTake = "LASER_BEAM";
        }

        ExecuteAction(actionToTake);
        thinkTimer = 1.5f; 
    }

    private void Think_FuSM_SetA()
    {
        if (!CheckTargets()) return;

        float distP1 = (player1 != null) ? Vector2.Distance(transform.position, player1.position) : Mathf.Infinity;
        float distP2 = (player2 != null) ? Vector2.Distance(transform.position, player2.position) : Mathf.Infinity;
        currentTarget = (distP1 < distP2) ? player1 : player2;
        float distance = Mathf.Min(distP1, distP2);

        var jarakFuzzy = new Dictionary<string, float> {
             { "Dekat",  Trimf(distance, -1f, 0f, 5f) },
             { "Sedang", Trimf(distance, 3f, 7f, 12f) },
             { "Jauh",   Trimf(distance, 10f, 15f, 25f) }
        };

        var urgencies = new Dictionary<string, float>();
        urgencies["GROUND_SLAM"] = jarakFuzzy["Dekat"] * 0.9f;
        urgencies["CHASE_PLAYER"] = jarakFuzzy["Sedang"] * 0.8f;
        urgencies["LASER_BEAM"] = jarakFuzzy["Jauh"] * 0.9f;
        
        urgencies["CHASE_PLAYER"] += 0.1f; 

        string chosenAction = ChooseWeightedRandomAction(urgencies);
        ExecuteAction(chosenAction);
        thinkTimer = 1.5f; 
    }

    private void Think_FuSM_SetB()
    {
        if (!CheckTargets()) return;

        if (player1 != null && player2 != null) 
            currentTarget = (Random.Range(0, 2) == 0) ? player1 : player2;
        else 
            currentTarget = (player1 != null) ? player1 : player2;

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        var jarakFuzzy = new Dictionary<string, float> {
             { "Dekat",  Trimf(distance, -1f, 0f, 5f) },
             { "Sedang", Trimf(distance, 3f, 6.5f, 10f) },
             { "Jauh",   Trimf(distance, 8f, 12f, 15f) }
        };
        float hpPercent = health / maxHealth;
        var hpFuzzy = new Dictionary<string, float> {
             { "Kritis", Trimf(hpPercent, -0.1f, 0f, 0.4f) },
             { "Sehat", Trimf(hpPercent, 0.3f, 0.7f, 1.1f) }
        };
        float stamPercent = currentStamina / maxStamina;
        var stamFuzzy = new Dictionary<string, float> {
            { "Penuh", Trimf(stamPercent, 0.5f, 1.0f, 1.1f) },
            { "Lelah", Trimf(stamPercent, -0.1f, 0.0f, 0.6f) }
        };

        bool canAttack = currentStamina >= staminaCostSlam || currentStamina >= staminaCostLaser || currentStamina >= (staminaCostChase * 2f);
        if (!canAttack) isRecharging = true; 
        else if (isRecharging && currentStamina >= rechargeThreshold) isRecharging = false; 

        var urgencies = new Dictionary<string, float>();

        if (isRecharging) 
        {
            urgencies["REPOSITION"] = 0.5f;
            urgencies["SHIELD"] = 0.4f;
            urgencies["IDLE"] = 0.1f;

            if (repositionCooldownTimer > 0 || !isOnGround) urgencies["REPOSITION"] = 0;
            if (shieldCooldownTimer > 0 || isInvincible) urgencies["SHIELD"] = 0;
        }
        else 
        {
            urgencies["GROUND_SLAM"] = jarakFuzzy["Dekat"] * 0.9f * stamFuzzy["Penuh"];
            urgencies["LASER_BEAM"] = jarakFuzzy["Jauh"] * 0.85f * stamFuzzy["Penuh"];
            urgencies["CHASE_PLAYER"] = jarakFuzzy["Sedang"] * 0.7f * stamFuzzy["Penuh"]; 
            urgencies["REPOSITION"] = (0.15f + (jarakFuzzy["Dekat"] * hpFuzzy["Kritis"] * 0.5f));
            
            float shieldUrg = (hpFuzzy["Kritis"] * 1.0f);
            if (shieldCooldownTimer > 0 || isInvincible) shieldUrg = 0;
            urgencies["SHIELD"] = shieldUrg;

            if (repositionCooldownTimer > 0 || !isOnGround) urgencies["REPOSITION"] = 0;
        }

        var qualified = urgencies.Where(x => x.Value > aiUrgencyThreshold).ToDictionary(x => x.Key, x => x.Value); 
        string chosen = ChooseWeightedRandomAction(qualified);
        
        if (chosen == "FORCE_RECHARGE")
        {
            isRecharging = true;
            ExecuteAction("IDLE"); 
            thinkTimer = 0.5f;     
        }
        else
        {
            ExecuteAction(chosen);
            if (isRecharging) 
            {
                thinkTimer = 0.5f; 
            }
            else
            {
                thinkTimer = Mathf.Lerp(maxThinkInterval, minThinkInterval, hpFuzzy["Kritis"]);
            }
        }
    }
    // --- AKHIR LOGIKA THINKING ---

    private void ExecuteAction(string action)
    {
        currentAction = action;
        bool canExecute = true;

        if (currentMode == AIMode.FuSM_SetB)
        {
            if (isRecharging && (action == "REPOSITION" || action == "SHIELD")) 
            { 
                canExecute = true; 
            }
            else
            {
                if (action == "GROUND_SLAM" && currentStamina < staminaCostSlam) canExecute = false;
                if (action == "LASER_BEAM" && currentStamina < staminaCostLaser) canExecute = false;
            }
        }

        if (!canExecute) {
            ExecuteAction("IDLE"); 
            return;
        }

        switch (action)
        {
            case "CHASE_PLAYER":
                if (currentTarget == null) break;
                float spd = moveSpeed;
                float dir = (currentTarget.position.x > transform.position.x) ? 1f : -1f;
                rb.velocity = new Vector2(dir * spd, rb.velocity.y);
                break;

            case "GROUND_SLAM":
                rb.velocity = Vector2.zero;
                if(currentMode == AIMode.FuSM_SetB) currentStamina -= staminaCostSlam;
                if (anim != null) anim.SetTrigger("AttackSlam");
                Instantiate(groundSlamPrefab, groundSlamPoint.position, Quaternion.identity)
                    .GetComponent<BossAttackHitbox>().owner = this;
                break;

            case "LASER_BEAM":
                rb.velocity = Vector2.zero;
                if(currentMode == AIMode.FuSM_SetB) currentStamina -= staminaCostLaser;
                if (anim != null) anim.SetTrigger("AttackLaser");
                if (currentTarget == null) return;
                Vector2 dirTo = (currentTarget.position - laserFirePoint.position).normalized;
                float angle = Mathf.Atan2(dirTo.y, dirTo.x) * Mathf.Rad2Deg;
                Instantiate(laserBeamPrefab, laserFirePoint.position, Quaternion.Euler(0,0,angle))
                    .GetComponent<LaserBeam>().owner = this;
                break;

            case "REPOSITION":
                if (currentTarget == null) return;
                if (anim != null) anim.SetTrigger("Jump");
                float hDir = (currentTarget.position.x > transform.position.x) ? 1f : -1f;
                if (isRecharging) hDir *= -1f; 
                rb.velocity = new Vector2(hDir * moveSpeed, repositionJumpForce); 
                repositionCooldownTimer = repositionCooldown; 
                isOnGround = false;
                break;

            case "SHIELD":
                // --- DEBUGGING SHIELD START ---
                if (shieldVisualPrefab == null || shieldSpawnPoint == null)
                {
                    Debug.LogError("BOSS SHIELD GAGAL: shieldVisualPrefab atau shieldSpawnPoint belum di-assign di Inspector!");
                    return; // Hentikan eksekusi jika referensi null
                }
                Debug.Log("BOSS SHIELD: Mengaktifkan perisai visual.");

                rb.velocity = Vector2.zero;
                isInvincible = true;
                shieldCooldownTimer = shieldCooldown; 
                
                // Instansiasi sebagai anak dari shieldSpawnPoint
                var shield = Instantiate(shieldVisualPrefab, shieldSpawnPoint);
                
                // Pastikan shield berada di local position (0,0,0) di bawah SpawnPoint
                // Ini penting jika SpawnPoint sudah berada di posisi yang benar di Boss
                shield.transform.localPosition = Vector3.zero; 
                
                Invoke(nameof(DisableShield), shieldDuration); // Menggunakan shieldDuration
                break;
            // --- DEBUGGING SHIELD END ---

            case "IDLE":
                rb.velocity = new Vector2(0, rb.velocity.y);
                break;
        }
    }

    private void UpdateStaminaLogic()
    {
        bool isPassive = currentAction == "IDLE" || currentAction == "SHIELD" || Mathf.Abs(rb.velocity.x) < 0.1f;

        if (currentAction == "CHASE_PLAYER")
        {
            currentStamina = Mathf.Max(0, currentStamina - staminaCostChase * Time.deltaTime);
        }
        else if (isPassive || isRecharging) 
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
        }
    }

    private void UpdateTimers()
    {
        if (shieldCooldownTimer > 0) shieldCooldownTimer -= Time.deltaTime;
        if (repositionCooldownTimer > 0) repositionCooldownTimer -= Time.deltaTime;
        if (currentMode == AIMode.FSM_Conventional) fsmRepositionTimer += Time.deltaTime;
    }

    private bool CheckTargets() { if (player1 == null && player2 == null) { this.enabled = false; rb.velocity = Vector2.zero; return false; } return true; }
    
    private void DisableShield() 
    { 
        isInvincible = false; 
        // --- DEBUGGING SHIELD CLEANUP ---
        if (shieldSpawnPoint != null)
        {
            if (shieldSpawnPoint.childCount > 0) 
            {
                // Menghapus objek anak (shield) yang pertama ditemukan
                Destroy(shieldSpawnPoint.GetChild(0).gameObject); 
                Debug.Log("BOSS SHIELD: Visual perisai dihapus.");
            }
        }
        // --- END CLEANUP ---
    } 
    
    public void TakeDamage(int damage) 
    { 
        if (isInvincible) return; 
        
        health -= damage; 
        
        if (anim != null) anim.SetTrigger("Hurt");

        if (health < 0) health = 0;

        if (health <= 0) 
        {
            if (anim != null) anim.SetTrigger("Death");

            this.enabled = false;
            rb.velocity = Vector2.zero;
            if(GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = false;

            Destroy(gameObject, 1.0f); 
        }
    }
    
    // ... (Fuzzy Logic Helpers and API methods remain unchanged)
    private float Trimf(float x, float a, float b, float c) { if (x < a || x > c) return 0; if (x < b) return (x - a) / (b - a); return (c - x) / (c - b); }
    
    private string ChooseWeightedRandomAction(Dictionary<string, float> options) {
        if (options.Count == 0) return "FORCE_RECHARGE"; 
        float total = options.Values.Sum(); 
        float rnd = Random.Range(0, total);
        foreach (var opt in options) { if (rnd < opt.Value) return opt.Key; rnd -= opt.Value; }
        return options.Keys.First();
    }
    
    private void OnCollisionEnter2D(Collision2D collision) { if (collision.gameObject.CompareTag("Ground")) isOnGround = true; }
    private void OnCollisionExit2D(Collision2D collision) { if (collision.gameObject.CompareTag("Ground")) isOnGround = false; }
    
    // ==================================================================================
    // NEW PUBLIC API FOR UI & DEBUG
    // ==================================================================================
    public float GetStaminaPercent() => currentStamina / maxStamina;
    public bool IsBossRecharging() => isRecharging;
    public string GetCurrentModeName() => currentMode.ToString();
    public string GetCurrentState() => currentAction;
    public float GetDistanceToTarget() => (currentTarget != null) ? Vector2.Distance(transform.position, currentTarget.position) : 0f;
    public float GetThinkTimer() => thinkTimer;
    public float GetHighestFuzzyScore() => 0f; // Placeholder

    public string GetDebugInfo()
    {
        string modeName = currentMode.ToString();
        string targetName = (currentTarget != null) ? currentTarget.name : "None";
        float dist = GetDistanceToTarget();

        string staminaColor = isRecharging ? "red" : "white";
        string stateColor = "yellow";
        string modeColor = "cyan";
        string hpColor = (health < maxHealth * 0.3f) ? "red" : "green";

        return $"<b>[AI ANALYTICS - SENTINEL]</b>\n" +
               $"Mode: <color={modeColor}>{modeName}</color>\n" +
               $"Action: <color={stateColor}>{currentAction}</color>\n" +
               $"HP: <color={hpColor}>{health}/{maxHealth}</color>\n" +
               $"Stamina: <color={staminaColor}>{currentStamina:F0}/{maxStamina}</color>\n" +
               $"Recharging: <b>{isRecharging}</b>\n" +
               $"Target: {targetName} (Dist: {dist:F1}m)\n" +
               $"Think Timer: {thinkTimer:F2}s";
    }
}