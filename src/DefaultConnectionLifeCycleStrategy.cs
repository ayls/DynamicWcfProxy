using System;
using System.Threading;

namespace Ayls.DynamicWcfProxy
{
    public class DefaultConnectionLifeCycleStrategy<T> : ConnectionLifeCycleStrategyBase<T>
        where T : class 
    {
        private readonly object _connectionLock = new object();
        private DateTime _lastUsed;
        private Timer _timer;
        private int? _maxConnectionIdleTime;
        private int MaxConnectionIdleTime
        {
            get
            {
                if (_maxConnectionIdleTime == null)
                    // take send timeout (operation timeout) and add 5 seconds on top of it to get the idle time after which connection should be closed
                    // this is to avoid closing the connection before it timeouts which could lead to confusing error messages
                    _maxConnectionIdleTime = Convert.ToInt32(EndpointContext.ServiceFactory.Endpoint.Binding.SendTimeout.TotalMilliseconds) + 5000;

                return _maxConnectionIdleTime.Value;
            }
        }

        public override T Open()
        {
            lock (_connectionLock)
            {
                var proxyOut = base.Open();

                _lastUsed = DateTime.Now;
                StartConnectionCheck();

                return proxyOut;
            }
        }

        public override void Close()
        {
            lock (_connectionLock)
            {
                base.Close();
                StopConnectionCheck();
            }
        }

        private void StartConnectionCheck()
        {
            if (_timer == null)
                _timer = new Timer(ConnectionCheck, null, MaxConnectionIdleTime, MaxConnectionIdleTime);
            else
                _timer.Change(MaxConnectionIdleTime, MaxConnectionIdleTime);
        }

        private void ConnectionCheck(object state)
        {
            lock (_connectionLock)
            {
                DateTime checkAt = DateTime.Now;
                if ((checkAt - _lastUsed).TotalMilliseconds >= MaxConnectionIdleTime)
                {
                    base.Close();
                    StopConnectionCheck();
                }
            }
        }

        private void StopConnectionCheck()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
