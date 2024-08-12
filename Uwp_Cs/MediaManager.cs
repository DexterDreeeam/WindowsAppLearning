using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Management;
using Windows.Media.Control;

namespace Uwp_Cs
{
    using SessionManager = GlobalSystemMediaTransportControlsSessionManager;
    using Session = GlobalSystemMediaTransportControlsSession;

    internal class MediaManager : IDisposable
    {
        private readonly SemaphoreSlim _SessionManagerLock = new SemaphoreSlim(1, 1);
        private SessionManager _SessionManager = null;
        private SessionManager m_SessionManager
        {
            get
            {
                _SessionManagerLock.Wait();
                try
                {
                    return _SessionManager;
                }
                finally
                {
                    _SessionManagerLock.Release();
                }
            }
            set
            {
                _SessionManagerLock.Wait();
                try
                {
                    if (_SessionManager != null)
                    {
                        _SessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
                    }
                    _SessionManager = value;
                    if (_SessionManager != null)
                    {
                        _SessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                    }
                }
                finally
                {
                    _SessionManagerLock.Release();
                }
            }
        }

        private readonly SemaphoreSlim _SessionLock = new SemaphoreSlim(1, 1);
        private Session _Session = null;
        private Session m_Session
        {
            get
            {
                _SessionLock.Wait();
                try
                {
                    if (_Session != null)
                    {
                        return _Session;
                    }

                    _Session = m_SessionManager.GetCurrentSession();
                    _Session.PlaybackInfoChanged += OnPlaybackInfoChanged;
                    return _Session;
                }
                finally
                {
                    _SessionLock.Release();
                }
            }
            set
            {
                _SessionLock.Wait();
                try
                {
                    if (_Session != null)
                    {
                        _Session.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                    }
                    _Session = value;
                    if (_Session != null)
                    {
                        _Session.PlaybackInfoChanged += OnPlaybackInfoChanged;
                    }
                }
                finally
                {
                    _SessionLock.Release();
                }
            }
        }

        private readonly object _LogsLock = new object();
        private List<string> m_Logs = new List<string>();

        private void Logging(string log)
        {
            lock (_LogsLock)
            {
                if (m_Logs.Count >= 100)
                {
                    m_Logs.Clear();
                }
                string ts = DateTime.Now.ToString("HH:mm:ss.fff");
                m_Logs.Add($"[{ts}] {log}");
            }
        }

        private void OnCurrentSessionChanged(SessionManager sender, CurrentSessionChangedEventArgs args)
        {
            Logging($"OnCurrentSessionChanged: {args}");
            m_Session = sender.GetCurrentSession();
        }

        private void OnPlaybackInfoChanged(Session sender, PlaybackInfoChangedEventArgs args)
        {
            var info = m_Session.GetPlaybackInfo();
            Logging($"OnPlaybackInfoChanged: {info.PlaybackStatus}");
        }

        public MediaManager()
        {
        }

        public async Task InitializeAsync()
        {
            if (m_SessionManager == null)
            {
                var sessionManager = await SessionManager.RequestAsync();
                sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                m_SessionManager = sessionManager;
            }
        }

        public void Dispose()
        {
            m_Session = null;
            m_SessionManager = null;
        }

        public async Task<bool> PlayAsync()
        {
            Logging($"Calling PlayMedia");
            return await m_Session.TryPlayAsync();
        }

        public async Task<bool> PauseAsync()
        {
            Logging($"Calling PauseMedia");
            return await m_Session.TryPauseAsync();
        }
    }
}
