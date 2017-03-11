using System;

namespace UBS.FundManager.Service.EventComponents
{
    /// <summary>
    /// Exposes the operations on a component
    /// </summary>
    public interface IListeningComponent : IDisposable
    {
        void Start();
        void Stop();
    }
}
