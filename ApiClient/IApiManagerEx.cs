using System;
using SharedTemplate;

namespace RestApiClient
{
    public interface IApiManagerEx<TData> where TData : IId
    {
        IObservable<TData> EventSequence { get; }

        void SubmitDistinct(TData obj, Func<TData, TData, bool> areSimilar);
    }
}