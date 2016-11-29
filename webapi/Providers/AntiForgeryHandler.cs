using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace webapi
{
    internal class AntiForgeryHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
        {
            var cookie = string.Empty;
            var form = string.Empty;

            //IEnumerable<string> antiForgeryHeaders;
            string[] skipApis = { "/api/Login/Token", "/api/Login/Signup", "/api/Login/Login" };
            //if (!skipApis.Any(i => i == request.RequestUri.LocalPath))
            //{
            //    if (request.Headers.TryGetValues("antiForgeryToken", out antiForgeryHeaders))
            //    {
            //        var tokens = antiForgeryHeaders.First();
            //        Token token = new Token();
            //        if (token.validate(tokens, ModelUtil.GetClientIp(request)))
            //        {
            //            return base.SendAsync(request, cancellationToken);
            //        }
            //    }

            //    var res = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            //    {
            //        Content = new StringContent("Illigal request!")
            //    };
            //    return Task.FromResult(res);

            //}
            return base.SendAsync(request, cancellationToken);
        }
    }
}
