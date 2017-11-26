using SharedTemplate;

namespace RestApiClient
{
    public interface IApiManager<TData> where TData : IId
    {
        void SubmitObject(TData obj);
    }
}