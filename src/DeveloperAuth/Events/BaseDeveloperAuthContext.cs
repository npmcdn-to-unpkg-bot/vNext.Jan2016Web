// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Authentication.DeveloperAuth
{
    /// <summary>
    /// Base class for other DeveloperAuth contexts.
    /// </summary>
    public class BaseDeveloperAuthContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="BaseDeveloperAuthContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="options">The options for DeveloperAuth</param>
        public BaseDeveloperAuthContext(HttpContext context, DeveloperAuthOptions options)
            : base(context)
        {
            Options = options;
        }

        public DeveloperAuthOptions Options { get; }
    }
}
