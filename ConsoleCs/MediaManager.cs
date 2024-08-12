using Windows.Media.Control;

namespace Uwp_Cs
{
    using SessionManager = GlobalSystemMediaTransportControlsSessionManager;
    using Session = GlobalSystemMediaTransportControlsSession;

    internal class MediaManager : IDisposable
    {
        private readonly SemaphoreSlim _SessionManagerLock = new SemaphoreSlim(1, 1);
        private SessionManager? _SessionManager = null;
        private SessionManager? m_SessionManager
        {
            get
            {
                _SessionManagerLock.Wait();
                try
                {
                    if (_SessionManager != null)
                    {
                        return _SessionManager;
                    }
                    _SessionManager = SessionManager.RequestAsync().GetResults();
                    _SessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
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
        private Session? _Session = null;
        private Session? m_Session
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

        private void OnCurrentSessionChanged(SessionManager sender, CurrentSessionChangedEventArgs args)
        {
            Console.WriteLine($"OnCurrentSessionChanged: {args}");
            m_Session = sender.GetCurrentSession();
        }

        private void OnPlaybackInfoChanged(Session sender, PlaybackInfoChangedEventArgs args)
        {
            Console.WriteLine($"OnPlaybackInfoChanged: {args}");
        }

        public MediaManager()
        {
        }

        public void Dispose()
        {
            m_Session = null;
            m_SessionManager = null;
        }

        public bool PlayMedia()
        {
            Console.WriteLine($"Calling PlayMedia");
            return m_Session.TryPlayAsync().GetResults();
        }

        public bool PauseMedia()
        {
            Console.WriteLine($"Calling PauseMedia");
            return m_Session.TryPauseAsync().GetResults();
        }
    }
}
