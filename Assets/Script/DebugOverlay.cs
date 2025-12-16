using UnityEngine;
using TMPro;

public class DebugOverlay : MonoBehaviour
{
    [Header("Dependencies")]
    public BossAI bossAI;           
    public GameObject overlayPanel; 
    public TextMeshProUGUI infoText;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.BackQuote; // Tombol ` (sebelah angka 1)

    void Start() {
        if(overlayPanel != null) overlayPanel.SetActive(false); // Default OFF
    }

    void Update() {
        // Toggle Input
        if (Input.GetKeyDown(toggleKey)) {
            if (overlayPanel != null) overlayPanel.SetActive(!overlayPanel.activeSelf);
        }

        // Update Text
        if (overlayPanel != null && overlayPanel.activeSelf) {
            if (bossAI == null) {
                bossAI = FindObjectOfType<BossAI>(); // Cari ulang jika null
                return;
            }
            if (infoText != null) infoText.text = bossAI.GetDebugInfo();
        }
    }
}