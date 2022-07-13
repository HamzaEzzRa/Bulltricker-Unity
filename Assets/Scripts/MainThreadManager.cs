using System;
using System.Collections.Generic;
using UnityEngine;

internal class MainThreadManager : MonoBehaviour
{
    #region Members
    internal static MainThreadManager worker;
    Queue<Action> jobs = new Queue<Action>();
    #endregion

    #region Awake
    void Awake()
    {
        worker = this;
        DontDestroyOnLoad(this);
    }
    #endregion

    #region Update
    void Update()
    {
        while (jobs.Count > 0)
            try
            {
                jobs.Dequeue().Invoke();
            }
            catch (Exception exception)
            {
                AuthenticationManager.instance.AddToLog(exception.ToString());
            }
    }
    #endregion

    #region AddJob
    internal void AddJob(Action newJob)
    {
        jobs.Enqueue(newJob);
    }
    #endregion
}