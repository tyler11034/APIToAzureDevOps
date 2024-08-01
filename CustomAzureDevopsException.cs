using System.Net;

public class CustomAzureDevOpsException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string ErrorDetails { get; }

    public CustomAzureDevOpsException()
    {
    }

    public CustomAzureDevOpsException(string message)
        : base(message)
    {
    }

    public CustomAzureDevOpsException(string message, Exception inner)
        : base(message, inner)
    {
    }

    public CustomAzureDevOpsException(string message, Exception inner, HttpStatusCode statusCode, string errorDetails)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ErrorDetails = errorDetails;
    }
}