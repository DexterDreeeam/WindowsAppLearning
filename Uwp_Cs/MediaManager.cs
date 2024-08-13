using System;
using System.Collections.Concurrent;
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
    using PlaybackStatus = GlobalSystemMediaTransportControlsSessionPlaybackStatus;

    internal class MediaManager : IDisposable
    {
        private readonly SemaphoreSlim _SessionManagerLock = new SemaphoreSlim(1, 1);
        private SessionManager _SessionManager = null;
        private SessionManager m_SessionManager
        {
            get
            {
                return _SessionManager;
            }
            set
            {
                _SessionManagerLock.Wait();
                try
                {
                    m_Session = null;
                    if (_SessionManager != null)
                    {
                        _SessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
                    }
                    _SessionManager = value;
                    if (_SessionManager != null)
                    {
                        m_Session = _SessionManager.GetCurrentSession();
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
                return _Session;
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

        private ConcurrentQueue<string> m_Logs = new ConcurrentQueue<string>();

        private void Logging(string log)
        {
            if (m_Logs.Count >= 200)
            {
                m_Logs.TryDequeue(out var _);
            }
            string ts = DateTime.Now.ToString("HH:mm:ss.fff");
            m_Logs.Enqueue($"[{ts}] {log}");
        }

        private void OnCurrentSessionChanged(SessionManager sender, CurrentSessionChangedEventArgs args)
        {
            Logging($"OnCurrentSessionChanged: {args}");
            m_Session = sender.GetCurrentSession();
        }

        private void OnPlaybackInfoChanged(Session sender, PlaybackInfoChangedEventArgs args)
        {
            if (m_Session != null)
            {
                var info = m_Session.GetPlaybackInfo();
                Logging($"OnPlaybackInfoChanged: {info.PlaybackStatus}");
            }
        }

        public MediaManager()
        {
        }

        public async Task InitializeAsync()
        {
            if (m_SessionManager == null)
            {
                var sessionManager = await SessionManager.RequestAsync().AsTask().ConfigureAwait(false);
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
            if (m_Session != null)
            {
                Logging($"Calling PlayMedia");
                return await m_Session.TryPlayAsync().AsTask().ConfigureAwait(false);
            }
            return false;
        }

        public async Task<bool> PauseAsync()
        {
            if (m_Session != null)
            {
                Logging($"Calling PauseMedia");
                return await m_Session.TryPauseAsync().AsTask().ConfigureAwait(false);
            }
            return false;
        }

        public async Task<bool> ToggleAsync()
        {
            if (m_Session != null)
            {
                var info = m_Session.GetPlaybackInfo();
                if (info.PlaybackStatus == PlaybackStatus.Playing)
                {
                    return await PauseAsync().ConfigureAwait(false);
                }
                else
                {
                    return await PlayAsync().ConfigureAwait(false);
                }
            }
            return false;
        }
    }
}
