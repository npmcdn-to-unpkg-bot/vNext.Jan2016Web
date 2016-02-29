using System.ComponentModel;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.OptionsModel;

namespace Pingo.AspNetCore.Authentication.Developer
{
    public class DeveloperOptions : RemoteAuthenticationOptions
    {
        public DeveloperOptions()
        {
            AuthenticationScheme = DeveloperDefaults.AuthenticationScheme;
            DisplayName = AuthenticationScheme;
            Events = new DeveloperEvents();
            CallbackPath = new PathString("/signin-developer");
        }
        public DeveloperOptions Value { get; }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public static ISecureDataFormat<RequestToken> StateDataFormat { get; set; }
        /// <summary>
        /// Gets or sets the <see cref="IDeveloperEvents"/> used to handle authentication events.
        /// </summary>
        public new IDeveloperEvents Events
        {
            get { return (IDeveloperEvents)base.Events; }
            set { base.Events = value; }
        }
        /// <summary>
        /// For testing purposes only.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ISystemClock SystemClock { get; set; } = new SystemClock();

    }
}