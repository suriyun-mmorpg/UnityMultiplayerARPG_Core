using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class ThreadedJob
{
    private bool m_IsDone = false;
    private bool m_IsError = false;
    private Exception m_Exception = null;
    private object m_Handle = new object();
    private Thread m_Thread = null;

    public bool IsDone
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsDone;
            }
            return tmp;
        }
        private set
        {
            lock (m_Handle)
            {
                m_IsDone = value;
            }
        }
    }

    public bool IsError
    {
        get
        {
            bool tmp;
            lock (m_Handle)
            {
                tmp = m_IsError;
            }
            return tmp;
        }
        private set
        {
            lock (m_Handle)
            {
                m_IsError = value;
            }
        }
    }

    public Exception Exception
    {
        get
        {
            Exception tmp;
            lock (m_Handle)
            {
                tmp = m_Exception;
            }
            return tmp;
        }
        private set
        {
            lock (m_Handle)
            {
                m_Exception = value;
            }
        }
    }

    public virtual void Start()
    {
        m_Thread = new Thread(Run);
        m_Thread.Start();
    }

    public virtual void Abort()
    {
        m_Thread.Abort();
    }

    protected virtual void ThreadFunction() { }

    protected virtual void OnFinished() { }

    public virtual bool Update()
    {
        if (IsDone)
        {
            OnFinished();
            return true;
        }
        return false;
    }

    public IEnumerator WaitFor()
    {
        while (!Update())
        {
            yield return 0;
        }
        if (IsError)
        {
            Debug.LogError("[ThreadedJob] Error occurs on " + GetType().Name);
            Debug.LogException(Exception);
        }
    }

    private void Run()
    {
        IsDone = false;
        IsError = false;
        Exception = null;
        try
        {
            ThreadFunction();
        }
        catch (Exception ex)
        {
            IsError = true;
            Exception = ex;
        }
        IsDone = true;
    }
}
