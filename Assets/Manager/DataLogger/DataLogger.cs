using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    public static DataLogger Instance { get; private set; }

    // ============================================
    // Enemy_Script.cs - COMPLETE UPDATED VERSION
    // ============================================

    [Header("Session Settings")]
    public string participantID = "P001";
    public string aiType = "Traditional"; // "Traditional" or "Adaptive"
    public int sessionNumber = 1; // 1 for first AI, 2 for second AI

    private List<LogEntry> logEntries = new List<LogEntry>();
    private float sessionStartTime;
    private float lastPlayerAttackTime = 0f;

    [System.Serializable]
    public class LogEntry
    {
        public string ParticipantID;
        public string AIType;
        public int SessionNumber;
        public float Timestamp;
        public string EventType;
        public string EventDetails;
        public float TimeSinceLastAttack;

        public LogEntry(string participantID, string aiType, int sessionNumber, float timestamp,
                       string eventType, string eventDetails = "", float timeSinceLastAttack = 0f)
        {
            ParticipantID = participantID;
            AIType = aiType;
            SessionNumber = sessionNumber;
            Timestamp = timestamp;
            EventType = eventType;
            EventDetails = eventDetails;
            TimeSinceLastAttack = timeSinceLastAttack;
        }
    }

    void Awake()
    {
        // Singleton pattern
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

    void Start()
    {
        sessionStartTime = Time.time;
        Debug.Log($"[DataLogger] Session started - Participant: {participantID}, AI: {aiType}");
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
        Debug.Log($"[LOG] Player Death");
    }

    // ==================== AI ACTION LOGGING ====================

    private float lastAIAttackTime = 0f;

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
        // damageType: "Full", "Blocked", "Avoided"
        string details = $"{damageType}|{damageAmount}|Dist:{distance:F2}";
        AddLog("AITakeDamage", details);
        Debug.Log($"[LOG] AI Damage - {damageType}: {damageAmount} | Distance: {distance:F2}");
    }

    public void LogCombatStart()
    {
        AddLog("CombatStart");
        Debug.Log("[LOG] Combat Started");
    }

    public void LogAIDeath(float timeSurvived)
    {
        string details = $"TimeSurvived:{timeSurvived:F2}";
        AddLog("AIDeath", details);
        Debug.Log($"[LOG] AI Death | Time Survived: {timeSurvived:F2}s");
    }

    // ==================== AI DECISION LOGGING (Neural Network) ====================

    public void LogAdaptation(string adaptationType, float triggerValue, float distance)
    {
        string logEntry = $"{Time.time:F2},ADAPTATION,{adaptationType},{triggerValue:F3},{distance:F2}";
        Debug.Log($"[DATA] {logEntry}");
        // TODO: Write to your CSV/file
    }

    public void LogPlayerAction(string actionType, float distance)
    {
        string logEntry = $"{Time.time:F2},PLAYER_ACTION,{actionType},{distance:F2}";
        Debug.Log($"[DATA] {logEntry}");
        // TODO: Write to your CSV/file
    }

    public void LogAIDecision(string actionName, float confidence, float distance = 0f)
    {
        string details = $"{actionName}|Conf:{confidence:F3}|Dist:{distance:F2}";
        AddLog("AIDecision", details);
        Debug.Log($"[LOG] AI Decision - {actionName} | Confidence: {confidence:F3} | Distance: {distance:F2}");
    }

    // ==================== AI MOVEMENT LOGGING ====================

    public void LogAIMovement(string movementType, float distance = 0f)
    {
        // movementType: "TowardPlayer", "AwayFromPlayer", "Stop", "Left", "Right"
        string details = $"{movementType}|Dist:{distance:F2}";
        AddLog("AIMovement", details);
    }

    // ==================== CORE LOGGING FUNCTION ====================

    private void AddLog(string eventType, string eventDetails = "", float timeSinceLastAttack = 0f)
    {
        float timestamp = Time.time - sessionStartTime;
        LogEntry entry = new LogEntry(
            participantID,
            aiType,
            sessionNumber,
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
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"GameplayData_{participantID}_{aiType}_Session{sessionNumber}_{timestamp}.csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write header
                writer.WriteLine("ParticipantID,AIType,SessionNumber,Timestamp,EventType,EventDetails,TimeSinceLastAttack");

                // Write all log entries
                foreach (LogEntry entry in logEntries)
                {
                    writer.WriteLine($"{entry.ParticipantID},{entry.AIType},{entry.SessionNumber}," +
                                   $"{entry.Timestamp:F3},{entry.EventType},{entry.EventDetails}," +
                                   $"{entry.TimeSinceLastAttack:F3}");
                }
            }

            Debug.Log($"[DataLogger] Data saved successfully to: {filePath}");
            Debug.Log($"[DataLogger] Total events logged: {logEntries.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLogger] Failed to save data: {e.Message}");
        }
    }

    public void ClearLogs()
    {
        logEntries.Clear();
        lastPlayerAttackTime = 0f;
        sessionStartTime = Time.time;
        Debug.Log("[DataLogger] Logs cleared - Ready for new session");
    }

    // Save data when application quits
    void OnApplicationQuit()
    {
        if (logEntries.Count > 0)
        {
            SaveToCSV();
        }
    }

    // Optional: Manual save with key press (useful for testing)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveToCSV();
        }
    }
}