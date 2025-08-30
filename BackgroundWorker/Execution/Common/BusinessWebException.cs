using System.Net;

namespace Pro4Soft.BackgroundWorker.Execution.Common;

public class BusinessWebException(HttpStatusCode code, string message = null, object payload = null, Exception innerException = null)
    : Exception(message, innerException)
{
    public HttpStatusCode Code { get; } = code;
    public object Payload { get; } = payload;
    
    public BusinessWebException(string message = null, object payload = null) : this(HttpStatusCode.NotAcceptable, message, payload)
    {
    }

    public BusinessWebException(HttpStatusCode code, string message = null) : this(code, message, null)
    {
        Code = code;
    }
}