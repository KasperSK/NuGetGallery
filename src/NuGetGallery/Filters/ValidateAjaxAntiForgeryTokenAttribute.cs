// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Web.Helpers;
using System.Web.Mvc;

namespace NuGetGallery.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateAjaxAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly HttpVerbs? _onVerbs;

        public ValidateAjaxAntiForgeryTokenAttribute()
        {
        }

        public ValidateAjaxAntiForgeryTokenAttribute(HttpVerbs onVerbs)
        {
            _onVerbs = onVerbs;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var request = filterContext.HttpContext.Request;

            if (_onVerbs.HasValue)
            {
                var parsed = Enum.TryParse<HttpVerbs>(request.HttpMethod, ignoreCase: true, result: out var verb);

                if ((parsed && !_onVerbs.Value.HasFlag(verb)) || !parsed)
                {
                    return;
                }
            }

            var cookie = request.Cookies[AntiForgeryConfig.CookieName];
            var headerValue = request.Headers[AntiForgeryConfig.CookieName];

            AntiForgery.Validate(cookie?.Value, headerValue);
        }
    }
}