using System;
using System.Collections.Generic;

[Serializable] public struct EnemyKillEntry  { public string name; public int   value; }
[Serializable] public struct SkillDpsEntry   { public string name; public float value; }

[Serializable]
public class PlaySessionData
{
    public string sessionId;
    public string sessionDate;
    public float sessionDurationSeconds;

    // ===== EnemyManager =====
    public int totalKills;
    public List<EnemyKillEntry> EnemyKillData = new();

    // ===== DpsManager =====
    public float totalLifetimeDps;
    public float totalDamageDealt;
    public List<SkillDpsEntry> SkillDamageData = new();

    // ===== Player =====
    public int finalPlayerLevel;
}