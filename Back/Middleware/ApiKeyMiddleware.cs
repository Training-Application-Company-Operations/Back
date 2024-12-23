﻿using Back.Interfaces;
using System.Net;

namespace Back.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string API_KEY_HEADER_NAME = "X-API-KEY";
        private const string UNAUTHORIZED_RESPONSE = "API Key was not provided.";
        private const string FORBIDDEN_RESPONSE = "Unauthorized client.";
        private const string API_KEY_CONFIG = "KV:ApiKey";
        private readonly IKeyVaultService _keyVaultService;
        private readonly IConfiguration _configuration;

        public ApiKeyMiddleware(RequestDelegate next, IKeyVaultService keyVaultService, IConfiguration configuration)
        {
            _next = next;
            _keyVaultService = keyVaultService;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var skipApiKeyAttribute = endpoint.Metadata.GetMetadata<SkipApiKeyAttribute>();
                if (skipApiKeyAttribute != null)
                {
                    await _next(context);
                    return;
                }
            }

            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync(UNAUTHORIZED_RESPONSE);
                return;
            }

            var apiKeySecret = _configuration.GetValue<string>(API_KEY_CONFIG);
            var apiKey = await _keyVaultService.GetSecretAsync(apiKeySecret!);
            if (apiKey == null || !apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync(FORBIDDEN_RESPONSE);
                return;
            }

            await _next(context);
        }
    }
}
