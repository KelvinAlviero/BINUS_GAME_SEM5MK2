using UnityEngine;
using System.Collections.Generic;

public class PlayerProfiler : MonoBehaviour
{
    // The "Memory"
    private Queue<string> actionHistory = new Queue<string>();
    private int historySize = 20;

    // The Stats (The Brain will read these!)
    public float jumpFrequency { get; private set; } // 0.0 to 1.0
    public float aggressionScore { get; private set; } // 0.0 to 1.0

    // Call this from your PLAYER CONTROLLER whenever the player does something
    public void RecordAction(string action) 
    {
        actionHistory.Enqueue(action);
        if (actionHistory.Count > historySize) actionHistory.Dequeue();
        CalculateStats();
    }

    void CalculateStats()
    {
        float total = actionHistory.Count;
        if (total == 0) return;

        // Example Logic: Count how many "Jumps" vs Total Actions
        float jumps = 0; 
        float attacks = 0;

        foreach(string act in actionHistory)
        {
            if(act == "Jump") jumps++;
            if(act == "Attack") attacks++;
        }

        jumpFrequency = jumps / total;
        aggressionScore = attacks / total;
    }
}