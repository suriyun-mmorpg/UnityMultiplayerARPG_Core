using System;
using System.Collections;
using System.Threading;
using LiteNetLibManager;

public class ThreadedJob
{
    private bool m_IsDone = false;
    private bool m_IsError = false;
    private Exception m_Exception = null;
#if !DISABLE_THREAD
    private Thread m_Thread = null;
#endif

    public bool IsDone
    {
        get
        {
            return m_IsDone;
        }
        private set
        {
            m_IsDone = value;
        }
    }

    public bool IsError
    {
        get
        {
            return m_IsError;
        }
        private set
        {
            m_IsError = value;
        }
    }

    public Exception Exception
    {
        get
        {
            return m_Exception;
        }
        private set
        {
            m_Exception = value;
        }
    }

    public virtual void Start()
    {
#if !DISABLE_THREAD
        m_Thread = new Thread(Run);
        m_Thread.Start();
#else
        Run();
#endif
    }

    public virtual void Abort()
    {
#if !DISABLE_THREAD
        m_Thread.Abort();
#endif
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
            Logging.LogError("[ThreadedJob] Error occurs on " + GetType().Name);
            Logging.LogException(Exception);
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
