using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Messenger.WebApi.Filters
{
    public class OptionsHttpMessageHandler : DelegatingHandler
    {
        protected Task<HttpResponseMessage> SendAsync2(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Options)
            {
                var apiExplorer = GlobalConfiguration.Configuration.Services.GetApiExplorer();

                var controllerRequested = request.GetRouteData().Values["controller"] as string;
                var supportedMethods = apiExplorer.ApiDescriptions.Where(d =>
                {
                    var controller = d.ActionDescriptor.ControllerDescriptor.ControllerName;
                    return string.Equals(
                        controller, controllerRequested, StringComparison.OrdinalIgnoreCase);
                })
                .Select(d => d.HttpMethod.Method)
                .Distinct();

                if (!supportedMethods.Any())
                    return Task.Factory.StartNew(
                        () => request.CreateResponse(HttpStatusCode.NotFound));

                return Task.Factory.StartNew(() =>
                {
                    var resp = new HttpResponseMessage(HttpStatusCode.OK);
                    resp.Headers.Add("Access-Control-Allow-Origin", "*");
                    resp.Headers.Add(
                        "Access-Control-Allow-Methods", string.Join(",", supportedMethods));

                    return resp;
                });
            }

            return base.SendAsync(request, cancellationToken);

        }
        protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Create the response.
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Hello!")
            };

            // Note: TaskCompletionSource creates a task that does not contain a delegate.
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);   // Also sets the task state to "RanToCompletion"
            return tsc.Task;
        }
    }
}