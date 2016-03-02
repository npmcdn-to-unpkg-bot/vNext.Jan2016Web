// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Authentication.Twitter2
{
    /// <summary>
    /// Base class for other Twitter2 contexts.
    /// </summary>
    public class BaseTwitter2Context : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="BaseTwitter2Context"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="options">The options for Twitter2</param>
        public BaseTwitter2Context(HttpContext context, Twitter2Options options)
            : base(context)
        {
            Options = options;
        }

        public Twitter2Options Options { get; }
    }
}
