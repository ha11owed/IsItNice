using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ClientLogic.WinRT.Extensions
{
    /// <summary>
    /// Add custom parameters to each request when applying this filter to a mobile service client.
    /// </summary>
    public class CustomParametersServiceFilter : IServiceFilter
    {
        private IDictionary<string, object> parameters;

        public CustomParametersServiceFilter(IDictionary<string, object> parameters)
        {
            this.parameters = parameters;
        }

        public IAsyncOperation<IServiceFilterResponse> Handle(IServiceFilterRequest request, IServiceFilterContinuation continuation)
        {
            // Get previous uri query
            var uriBuilder = new UriBuilder(request.Uri);
            var oldQuery = (uriBuilder.Query ?? string.Empty).Trim('?');

            // Build new query starting with our custom parameters before old query
            var stringBuilder = new StringBuilder();
            foreach (var parameter in parameters)
            {
                object val = parameter.Value;
                // Currently using ToString on the value, an improvement will be to serialize the object properly
                stringBuilder.AppendFormat("{0}={1}&", parameter.Key, val);
            }
            stringBuilder.Append(oldQuery);

            // Apply new query to request uri
            uriBuilder.Query = stringBuilder.ToString().Trim('&');
            request.Uri = uriBuilder.Uri;

            return continuation.Handle(request);
        }
    }

}
