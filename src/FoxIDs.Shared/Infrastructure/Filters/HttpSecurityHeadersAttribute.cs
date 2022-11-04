﻿using FoxIDs.Models.Config;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoxIDs.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class HttpSecurityHeadersAttribute : TypeFilterAttribute
    {
        public HttpSecurityHeadersAttribute() : base(typeof(HttpSecurityHeadersActionAttribute))
        { }
        public HttpSecurityHeadersAttribute(Type type) : base(type)
        { }

        public class HttpSecurityHeadersActionAttribute : IAsyncActionFilter
        {
            private const string ingestionEndpointKey = "IngestionEndpoint=";
            protected bool isHtmlContent;
            private readonly TelemetryScopedLogger logger;
            private readonly IWebHostEnvironment env;
            private readonly IServiceProvider serviceProvider;

            public HttpSecurityHeadersActionAttribute(TelemetryScopedLogger logger, IServiceProvider serviceProvider, IWebHostEnvironment env)
            {
                this.logger = logger;
                this.env = env;
                this.serviceProvider = serviceProvider;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var resultContext = await next();

                ActionExecutionInit(resultContext);
                SetHeaders(resultContext.HttpContext);
            }

            protected virtual void ActionExecutionInit(ActionExecutedContext resultContext)
            {
                isHtmlContent = resultContext.Result.IsHtmlContent();
            }

            protected virtual void SetHeaders(HttpContext httpContext)
            {
                logger.ScopeTrace(() => $"Adding http security headers. Is {(isHtmlContent ? string.Empty : "not")} view.");

                httpContext.Response.SetHeader("X-Content-Type-Options", "nosniff");
                httpContext.Response.SetHeader("Referrer-Policy", "no-referrer");
                httpContext.Response.SetHeader("X-XSS-Protection", "1; mode=block");

                if (isHtmlContent)
                {
                    HeaderXFrameOptions(httpContext);
                }

                var csp = CreateCsp(httpContext).ToSpaceList();
                if (!csp.IsNullOrEmpty())
                {
                    httpContext.Response.SetHeader("Content-Security-Policy", csp);
                    httpContext.Response.SetHeader("X-Content-Security-Policy", csp);
                }

                logger.ScopeTrace(() => $"Http security headers added.");
            }       

            protected virtual void HeaderXFrameOptions(HttpContext httpContext)
            {
                httpContext.Response.SetHeader("X-Frame-Options", "deny");
            }

            protected virtual IEnumerable<string> CreateCsp(HttpContext httpContext)
            {
                if (isHtmlContent)
                {
                    yield return "block-all-mixed-content;";

                    yield return "default-src 'self';";
                    yield return $"connect-src 'self' {GetConnectSrc(httpContext)};"; 

                    var cspImgSrc = CspImgSrc();
                    if (!cspImgSrc.IsNullOrEmpty())
                    {
                        yield return cspImgSrc;
                    }

                    yield return "script-src 'self' 'unsafe-inline' https://js.monitor.azure.com;";
                    yield return "style-src 'self' 'unsafe-inline';";

                    yield return "base-uri 'self';";

                    var cspFormAction = CspFormAction();
                    if (!cspFormAction.IsNullOrEmpty())
                    {
                        yield return cspFormAction;
                    }
                    var cspFrameSrc = CspFrameSrc();
                    if (!cspFrameSrc.IsNullOrEmpty())
                    {
                        yield return cspFrameSrc;
                    }

                    yield return "sandbox allow-forms allow-popups allow-same-origin allow-scripts;";

                    yield return CspFrameAncestors();
                }

                if (!env.IsDevelopment())
                {
                    yield return "upgrade-insecure-requests;";
                }
            }

            private string GetConnectSrc(HttpContext httpContext)
            {
                var applicationInsightsConnectSrc = GetApplicationInsightsConnectSrc(httpContext);
#if DEBUG
                if (env.IsDevelopment())
                {
                    return $"{applicationInsightsConnectSrc} {GetDevelopmentConnectSrc()}";
                }
#endif
                return applicationInsightsConnectSrc;
            }

            private string GetApplicationInsightsConnectSrc(HttpContext httpContext)
            {
                var connectSrc = GetIngestionEndpoint(httpContext.GetRouteBinding().TelemetryClient?.TelemetryConfiguration?.ConnectionString);

                if (connectSrc.IsNullOrWhiteSpace())
                {
                    var applicationInsightsSettings = serviceProvider.GetService<ApplicationInsightsGlobalSettings>();
                    connectSrc = GetIngestionEndpoint(applicationInsightsSettings.ConnectionString);
                }

                if (connectSrc.IsNullOrWhiteSpace())
                {
                    connectSrc = "https://dc.services.visualstudio.com/v2/track";
                }

                return connectSrc;
            }

            private string GetDevelopmentConnectSrc()
            {
                return "wss://localhost:44349/FoxIDs/";
            }

            protected virtual string CspImgSrc()
            {
                return "img-src 'self' data: 'unsafe-inline';";
            }

            protected virtual string CspFormAction()
            {
                return string.Empty;
            }

            protected virtual string CspFrameSrc()
            {
                return string.Empty;
            }            

            protected virtual string CspFrameAncestors()
            {
                return "frame-ancestors 'none';";
            }

            private string GetIngestionEndpoint(string connectionString)
            {
                if (connectionString.IsNullOrEmpty())
                {
                    return connectionString;
                }

                var conSplit = connectionString.Split(';');
                foreach (var item in conSplit)
                {
                    if (item.StartsWith(ingestionEndpointKey, StringComparison.OrdinalIgnoreCase))
                    {
                        return item.Substring(ingestionEndpointKey.Length);
                    }
                }
                return null;
            }
        }
    }
}
