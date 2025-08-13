using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using static FootballAgent;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public enum Team
{
    Kais = 0,
    Alberts = 1
}

public class FootballEnvController : MonoBehaviour
{
    #region Nested Types

    [Serializable]
    public class PlayerInfo
    {
        public FootballAgent Agent;
        [HideInInspector] public Vector3 StartingPos;
        [HideInInspector] public Quaternion StartingRot;
        [HideInInspector] public Rigidbody Rb;
    }

    #endregion

    #region Serialized Fields

    [Header("Enviroment Properties")]
    [SerializeField] private int ID;

    [Header("Timer Settings")]
    [SerializeField] private int timeLimit = 60; // Time limit in seconds

    [Header("References")]
    [SerializeField] private GameObject ball;

    #endregion

    #region Public Properties

    public int CurrentTime { get; private set; }
    public int IterationsCount => iterationsCount;

    #endregion

    #region Public Fields

    [HideInInspector] public Rigidbody ballRb;
    [HideInInspector] public StatsRecorder statsRecorder;
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    #endregion

    #region Events

    public event Action<int> OnIterationsCountChanged;
    public event Action<int, int, int> OnScoreChanged; // (envID, kaiScore, albertScore)
        
    #endregion

    #region Private Fields

    private float timer = 0f;
    private float elapsedTime = 0f;
    private Vector3 m_BallStartingPos;
    private int iterationsCount = 0;
    private bool episodeEnded = false;
    private int kaiScore = 0;
    private int albertScore = 0;

    #endregion

    #region Unity Methods

    private void Start()
    {
        Net.OnGoalScored += HandleGoalScored;
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = ball.transform.position;

        foreach(var i in AgentsList)
        {
            i.StartingPos = i.Agent.GetInitialPosition();
            i.Rb = i.Agent.GetComponent<Rigidbody>();
        }

        CurrentTime = timeLimit;
        timer = 0f;
        elapsedTime = 0f;
        iterationsCount = 0;
        episodeEnded = false;

        statsRecorder = Academy.Instance.StatsRecorder;

        ResetEnviroment();
    }

    private void FixedUpdate()
    {
        if (episodeEnded) return;

        HandleTimer();
    }

    #endregion

    #region Public Methods

    public void ResetBall()
    {
        if (ball == null)
        {
            Debug.LogWarning("Ball has been destroyed; skipping ResetBall");
            return;
        }

        var randomPosX = UnityEngine.Random.Range(-2.5f, 2.5f);
        var randomPosZ = UnityEngine.Random.Range(-2.5f, 2.5f);

        if (ball.transform == null) return; // transform destroyed

        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, randomPosZ);

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }
    }

    public void IncrementIterationsCount()
    {
        iterationsCount++;
        OnIterationsCountChanged?.Invoke(iterationsCount);
    }

    public void ResetIterationsCount()
    {
        iterationsCount = 0;
        OnIterationsCountChanged?.Invoke(iterationsCount);
    }

    public void ResetEnviroment()
{
    episodeEnded = false;

    // Remove destroyed agents from the list
    AgentsList.RemoveAll(a => a == null || a.Agent == null);

    foreach (var item in AgentsList)
    {
        if (item.Agent == null) continue; // skip destroyed agent

        var randomPosZ = UnityEngine.Random.Range(-5f, 5f);
        var randomPosX = UnityEngine.Random.Range(-1f, 1f);
        var newStartPos = item.Agent.GetInitialPosition() + new Vector3(randomPosX, 0f, randomPosZ);
        var rot = UnityEngine.Random.Range(80.0f, 120.0f);
        var newRot = Quaternion.Euler(0, rot, 0);

        if (item.Agent.transform != null)
        {
            item.Agent.transform.localPosition = newStartPos;
            item.Agent.transform.localRotation = newRot;
        }

        if (item.Rb != null)
        {
            item.Rb.linearVelocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }
    }

    ResetBall();

    CurrentTime = timeLimit;
    timer = 0f;
    elapsedTime = 0f;

    IncrementIterationsCount();
}

    public void SetTimeLimit(int limit)
    {
        timeLimit = limit;
        CurrentTime = limit;
        timer = 0f;
        elapsedTime = 0f;
    }

    public int GetTimeLimit() => timeLimit;

    public static int GetCurrentEnviromentID(GameObject gameObject)
    {
        GameObject parentObject = gameObject.transform.parent?.gameObject;
        while (parentObject != null && parentObject.GetComponent<FootballEnvController>() == null)
        {
            parentObject = parentObject.transform.parent?.gameObject;
        }
        return gameObject.GetComponentInParent<FootballEnvController>().GetEnviromentID();
    }

    public void SetEnviromentID(int id) => ID = id;

    public int GetEnviromentID() => ID;

    public int[] GetScore()
    {
        return new int[] { kaiScore, albertScore };
    }
    #endregion

    #region Private Methods

    private void HandleTimer()
    {
        if (episodeEnded) return;

        if (CurrentTime > 0)
        {
            timer += Time.fixedDeltaTime;
            elapsedTime += Time.fixedDeltaTime;
            if (timer >= 1f)
            {
                timer -= 1f;
                CurrentTime--;
            }
        }
        else
        {
            EndEpisodeByTimer();
        }
    }
    private void HandleGoalScored(object Sender, Net.OnGoalScoredEventArgs e)
    {
        if (e.envID != ID) return;

        if (e.TeamScored == Team.Kais)
        {
            kaiScore++;
        }
        else
        {
            albertScore++;
        }

        OnScoreChanged?.Invoke(ID, kaiScore, albertScore);

        foreach (var item in AgentsList)
        {
            item.Agent.HandleGoalScored(e);
        }

        ResetEnviroment();
    }

    private void EndEpisodeByTimer()
    {
        if (episodeEnded) return;
        episodeEnded = true;

        foreach(var i in AgentsList)
        {
            i.Agent.AddReward(Rewards.TimeLimitPenalty);
            i.Agent.EndEpisode();
        }

        statsRecorder.Add($"Reward_Team/Alberts", Rewards.TimeLimitPenalty, StatAggregationMethod.Sum);
        statsRecorder.Add($"Reward_Team/Kais", Rewards.TimeLimitPenalty, StatAggregationMethod.Sum);
        ResetEnviroment();
    }

    #endregion
}