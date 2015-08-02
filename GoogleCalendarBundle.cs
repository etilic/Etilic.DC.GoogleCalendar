using Etilic.Core.Extensibility;
using Etilic.Identity;
using Etilic.Identity.OAuth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etilic.DC.GoogleCalendar
{
    /// <summary>
    /// Google Calendar bundle for Etilic.
    /// </summary>
    public class GoogleCalendarBundle : Bundle
    {
        #region Constants
        /// <summary>
        /// The globally unique ID of this bundle.
        /// </summary>
        public const String GUID = "6DCAD5D3-6451-4B9F-AFFA-5AD9D3EAC4D2";
        #endregion

        #region Static Properties
        /// <summary>
        /// The globally unique ID of this bundle.
        /// </summary>
        public static Guid ID = new Guid(GUID);
        #endregion

        private GoogleCalendarAuthFlow flow;

        #region Properties
        /// <summary>
        /// Gets the globally unique ID of this bundle.
        /// </summary>
        public override Guid BundleID
        {
            get { return ID; }
        }
        /// <summary>
        /// Gets the globally unique IDs of this bundle's dependencies.
        /// </summary>
        public override Guid[] Dependencies
        {
            get
            {
                return new Guid[] {  
                    DCBundle.ID,
                    IdentityBundle.ID
                };
            }
        }
        #endregion

        #region OnAdded
        /// <summary>
        /// Invoked by <see cref="Etilic.Core.Extensibility.BundleManager"/> after the bundle
        /// has been registered.
        /// </summary>
        public override void OnAdded()
        {
            // get the identity bundle and register the Google Calendar OAuth flow
            var identityBundle = BundleManager.GetBundle<IdentityBundle>(IdentityBundle.ID);

            this.flow = new GoogleCalendarAuthFlow(this);
            identityBundle.RegisterOAuthFlow(this.flow);
        }
        #endregion

        #region CreateService
        /// <summary>
        /// Creates a <see cref="Google.Apis.v3.CalendarService"/> object for the given user
        /// and OAuth token.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public CalendarService CreateService(String userID, OAuthToken token)
        {
            // re-construct the TokenResponse object from our OAuthToken object
            // note that Google's API will use local time to verify that the
            // access token is still valid, so we need to convert it to that
            TokenResponse response = new TokenResponse();
            response.AccessToken = token.AccessToken;
            response.Issued = token.Issued.ToLocalTime();
            response.ExpiresInSeconds = (long)token.Expires.Subtract(token.Issued).TotalSeconds;
            response.TokenType = token.TokenType;

            BaseClientService.Initializer initialiser = new BaseClientService.Initializer();
            initialiser.HttpClientInitializer = new UserCredential(this.flow.Flow, userID, response);

            CalendarService service = new CalendarService(initialiser);

            return service;
        }
        #endregion
    }
}
