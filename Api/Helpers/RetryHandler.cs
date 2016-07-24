using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace MandraSoft.PokemonGo.Api.Helpers
{
    class RetryHandler : DelegatingHandler
    {
        private const int MaxRetries = 100;

        public RetryHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i <= MaxRetries; i++)
            {
                if(cancellationToken.IsCancellationRequested)return null;
                try
                {
                    var response = await base.SendAsync(request, cancellationToken);
                    if (response.StatusCode == HttpStatusCode.BadGateway)
                        throw new Exception(); //todo: proper implementation

                    return response;
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"retry request {request.RequestUri}");
                    if (i < MaxRetries)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    throw;
                }
            }
            return null;
        }
    }
}
