using System;

namespace HttpPack
{
    public class FetchResponse<T>
    {
        public FetchResponse(int responseStatusCode, string responseBody)
        {
            Code = responseStatusCode;
            Body = (T) Activator.CreateInstance(typeof(T), responseBody);
        }

        public int Code { get; }
        public T Body { get; }
    }
}