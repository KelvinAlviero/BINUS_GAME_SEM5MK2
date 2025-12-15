using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataLogger : MonoBehaviour
{
    public static DataLogger Instance { get; private set; }

    [Header("Session Settings")]
    public string participantID = "P001";
    public string aiType = "Traditional"; // "Traditional" or "Adaptive"
    public int sessionNumber = 0;
    public int attemptNumber = 1;

    private List<LogEntry> logEntries = new List<LogEntry>();
    private float sessionStartTime;
    private float lastPlayerAttackTime = 0f;
    private float lastAIAttackTime = 0f;
    private float combatStartTime = 0f;

    // NEW: Track previous scene to detect restarts vs new sessions
    private string previousSceneName = "";
    private string currentAIScene = "";

    [System.Serializable]
    public class LogEntry
    {
        public string ParticipantID;
        public string AIType;
        public int SessionNumber;
        public int AttemptNumber;
        public float Timestamp;
        public string EventType;
        public string EventDetails;
        public float TimeSinceLastAttack;

        public LogEntry(string participantID, string aiType, int sessionNumber, int attemptNumber,
                       float timestamp, string eventType, string eventDetails = "",
                       float timeSinceLastAttack = 0f)
        {
            ParticipantID = participantID;
            AIType = aiType;
            SessionNumber = sessionNumber;
            AttemptNumber = attemptNumber;
            Timestamp = timestamp;
            EventType = eventType;
            EventDetails = eventDetails;
            TimeSinceLastAttack = timeSinceLastAttack;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Skip if it's the main menu or non-gameplay scene
        if (scene.name == "MainMenu" || scene.name.Contains("Menu"))
        {
            previousSceneName = scene.name;
            Debug.Log($"[DataLogger] Menu scene loaded, skipping session setup");
            return;
        }

        // Save previous session if there's data
        if (logEntries.Count > 0)
        {
            SaveToCSV();
        }

        // Check if this is a restart (same scene) or new session (different scene)
        bool isRestart = (scene.name == currentAIScene && currentAIScene != "");

        if (isRestart)
        {
            // RESTART within same session - increment attempt number
            Debug.Log($"[DataLogger] RESTART DETECTED - Same scene: {scene.name}");
            StartNewAttempt();
        }
        else
        {
            // NEW SESSION - different scene or first load
            Debug.Log($"[DataLogger] NEW SESSION - Scene changed from '{previousSceneName}' to '{scene.name}'");

            // Detect AI type from scene name
            if (scene.name.Contains("Neural") || scene.name.Contains("Adaptive"))
            {
                aiType = "Adaptive";
            }
            else if (scene.name.Contains("State") || scene.name.Contains("Traditional"))
            {
                aiType = "Traditional";
            }

            // Increment session number for new gameplay session
            sessionNumber++;

            // Reset for new session
            ClearLogs();
            attemptNumber = 1;
            sessionStartTime = Time.time;

            Debug.Log($"[DataLogger] NEW SESSION - AI: {aiType} | Session: {sessionNumber}");
        }

        // Update tracking variables
        currentAIScene = scene.name;
        previousSceneName = scene.name;
    }

    void Start()
    {
        sessionStartTime = Time.time;
        Debug.Log($"[DataLogger] DataLogger initialized - Participant: {participantID}");
    }

    // ==================== PLAYER ACTION LOGGING ====================

    public void LogPlayerAttack(bool hitEnemy)
    {
        float currentTime = Time.time - sessionStartTime;
        float timeSinceLastAttack = (lastPlayerAttackTime > 0) ? currentTime - lastPlayerAttackTime : 0f;

        string details = hitEnemy ? "Hit" : "Miss";
        AddLog("PlayerAttack", details, timeSinceLastAttack);

        lastPlayerAttackTime = currentTime;
        Debug.Log($"[LOG] Player Attack - {details} | Time since last: {timeSinceLastAttack:F2}s");
    }

    public void LogPlayerDash()
    {
        AddLog("PlayerDash");
        Debug.Log($"[LOG] Player Dash");
    }

    public void LogPlayerJump()
    {
        AddLog("PlayerJump");
        Debug.Log($"[LOG] Player Jump");
    }

    public void LogPlayerDamage(float damageAmount, bool wasBlocked)
    {
        string details = wasBlocked ? $"Blocked|{damageAmount}" : $"Damaged|{damageAmount}";
        AddLog("PlayerTakeDamage", details);
        Debug.Log($"[LOG] Player Damage - Amount: {damageAmount} | Blocked: {wasBlocked}");
    }

    public void LogPlayerMovement(string direction)
    {
        AddLog("PlayerMovement", direction);
    }

    public void LogPlayerDeath()
    {
        AddLog("PlayerDeath");
        Debug.Log($"[LOG] Player Death - Attempt {attemptNumber} Failed");
    }

    // ==================== AI ACTION LOGGING ====================

    public void LogAIMeleeAttack(bool hitPlayer, float distance)
    {
        float currentTime = Time.time - sessionStartTime;
        float timeSinceLastAttack = (lastAIAttackTime > 0) ? currentTime - lastAIAttackTime : 0f;

        string details = $"{(hitPlayer ? "Hit" : "Miss")}|Dist:{distance:F2}";
        AddLog("AIMeleeAttack", details, timeSinceLastAttack);

        lastAIAttackTime = currentTime;
        Debug.Log($"[LOG] AI Melee Attack - {(hitPlayer ? "Hit" : "Miss")} | Distance: {distance:F2}");
    }

    public void LogAIRangeAttack(float distance)
    {
        float currentTime = Time.time - sessionStartTime;
        float timeSinceLastAttack = (lastAIAttackTime > 0) ? currentTime - lastAIAttackTime : 0f;

        string details = $"Dist:{distance:F2}";
        AddLog("AIRangeAttack", details, timeSinceLastAttack);

        lastAIAttackTime = currentTime;
        Debug.Log($"[LOG] AI Range Attack | Distance: {distance:F2}");
    }

    public void LogAIBlock(float distance, string blockType)
    {
        string details = $"{blockType}|Dist:{distance:F2}";
        AddLog("AIBlock", details);
        Debug.Log($"[LOG] AI Block - {blockType} | Distance: {distance:F2}");
    }

    public void LogAIDodge(float distance)
    {
        string details = $"Dist:{distance:F2}";
        AddLog("AIDodge", details);
        Debug.Log($"[LOG] AI Dodge | Distance: {distance:F2}");
    }

    public void LogAIDamage(float damageAmount, string damageType, float distance)
    {
        string details = $"{damageType}|{damageAmount}|Dist:{distance:F2}";
        AddLog("AITakeDamage", details);
        Debug.Log($"[LOG] AI Damage - {damageType}: {damageAmount} | Distance: {distance:F2}");
    }

    public void LogCombatStart()
    {
        combatStartTime = Time.time;
        AddLog("CombatStart");
        Debug.Log($"[LOG] Combat Started - Session {sessionNumber}, Attempt {attemptNumber}");
    }

    public void LogAIDeath(float timeSurvived)
    {
        string details = $"TimeSurvived:{timeSurvived:F2}";
        AddLog("AIDeath", details);
        Debug.Log($"[LOG] AI Death | Time Survived: {timeSurvived:F2}s");
    }

    // ==================== AI DECISION LOGGING ====================

    public void LogAdaptation(string adaptationType, float triggerValue, float distance)
    {
        string details = $"{adaptationType}|Trigger:{triggerValue:F3}|Dist:{distance:F2}";
        AddLog("AIAdaptation", details);
        Debug.Log($"[LOG] AI Adaptation - {adaptationType} | Trigger: {triggerValue:F3}");
    }

    public void LogPlayerAction(string actionType, float distance)
    {
        string details = $"{actionType}|Dist:{distance:F2}";
        AddLog("PlayerAction", details);
    }

    public void LogAIDecision(string actionName, float confidence, float distance = 0f)
    {
        string details = $"{actionName}|Conf:{confidence:F3}|Dist:{distance:F2}";
        AddLog("AIDecision", details);
        Debug.Log($"[LOG] AI Decision - {actionName} | Confidence: {confidence:F3}");
    }

    public void LogAIMovement(string movementType, float distance = 0f)
    {
        string details = $"{movementType}|Dist:{distance:F2}";
        AddLog("AIMovement", details);
    }

    // ==================== SESSION MANAGEMENT ====================

    public void EndCombat(string outcome)
    {
        // outcome: "PlayerVictory" or "PlayerDefeat"
        AddLog("CombatEnd", outcome);
        Debug.Log($"[DataLogger] ========================================");
        Debug.Log($"[DataLogger] COMBAT ENDED: {outcome}");
        Debug.Log($"[DataLogger] Session: {sessionNumber} | AI: {aiType} | Attempt: {attemptNumber}");
        Debug.Log($"[DataLogger] Events logged: {logEntries.Count}");
        Debug.Log($"[DataLogger] ========================================");

        // Save immediately after combat ends
        SaveToCSV();
    }

    public void StartNewAttempt()
    {
        // Increment attempt number for retry
        attemptNumber++;

        // Reset timers but keep session data
        sessionStartTime = Time.time;
        lastPlayerAttackTime = 0f;
        lastAIAttackTime = 0f;

        // Clear previous attempt's logs
        ClearLogs();

        Debug.Log($"[DataLogger] ----------------------------------------");
        Debug.Log($"[DataLogger] NEW ATTEMPT #{attemptNumber} - AI: {aiType} | Session: {sessionNumber}");
        Debug.Log($"[DataLogger] ----------------------------------------");
    }

    // ==================== CORE LOGGING FUNCTION ====================

    private void AddLog(string eventType, string eventDetails = "", float timeSinceLastAttack = 0f)
    {
        float timestamp = Time.time - sessionStartTime;
        LogEntry entry = new LogEntry(
            participantID,
            aiType,
            sessionNumber,
            attemptNumber,
            timestamp,
            eventType,
            eventDetails,
            timeSinceLastAttack
        );
        logEntries.Add(entry);
    }

    // ==================== CSV EXPORT ====================

    public void SaveToCSV()
    {
        if (logEntries.Count == 0)
        {
            Debug.Log("[DataLogger] No data to save");
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"GameplayData_{participantID}_{aiType}_S{sessionNumber}_A{attemptNumber}_{timestamp}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write header
                writer.WriteLine("ParticipantID,AIType,SessionNumber,AttemptNumber,Timestamp,EventType,EventDetails,TimeSinceLastAttack");

                // Write all log entries
                foreach (LogEntry entry in logEntries)
                {
                    writer.WriteLine($"{entry.ParticipantID},{entry.AIType},{entry.SessionNumber}," +
                                   $"{entry.AttemptNumber},{entry.Timestamp:F3},{entry.EventType}," +
                                   $"{entry.EventDetails},{entry.TimeSinceLastAttack:F3}");
                }
            }

            Debug.Log($"[DataLogger] ✅ DATA SAVED SUCCESSFULLY");
            Debug.Log($"[DataLogger] File: {fileName}");
            Debug.Log($"[DataLogger] Path: {filePath}");
            Debug.Log($"[DataLogger] Total events: {logEntries.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLogger] ❌ FAILED TO SAVE: {e.Message}");
        }
    }

    public void ClearLogs()
    {
        logEntries.Clear();
        lastPlayerAttackTime = 0f;
        lastAIAttackTime = 0f;
        sessionStartTime = Time.time;
        Debug.Log("[DataLogger] Logs cleared - Ready for new attempt");
    }

    void OnApplicationQuit()
    {
        if (logEntries.Count > 0)
        {
            SaveToCSV();
        }
    }

    void Update()
    {
        // Manual save with F9 (for testing)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveToCSV();
        }
    }
}