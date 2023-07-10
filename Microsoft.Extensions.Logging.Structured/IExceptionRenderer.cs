using System;

namespace Microsoft.Extensions.Logging.Structured;

public interface IExceptionRenderer
{
    string Render(Exception exception);
}