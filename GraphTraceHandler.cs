using Microsoft.Graph;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Graph.Community
{
    public class GraphTraceHandler : DelegatingHandler
    {
        private DiagnosticSource _logger = new DiagnosticListener("Microsoft.Graph.GraphTraceHandler");

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Activity activity= null;

            if (_logger.IsEnabled("MicrosoftGraphCall"))
            {
                activity = new Activity("MicrosoftGraphCall");
                activity.AddTag("method", request.Method.ToString());
                activity.AddTag("clientRequestId", request.GetRequestContext().ClientRequestId);
              
                _logger.StartActivity(activity, new { request });
            }
            
            var response = await base.SendAsync(request, cancellationToken);

            if (_logger.IsEnabled("MicrosoftGraphCall"))
            {
                activity.AddTag("statusCode", response.StatusCode.ToString());
                _logger.StopActivity(activity, new { response });
            }

            return response;
        }
    }
}