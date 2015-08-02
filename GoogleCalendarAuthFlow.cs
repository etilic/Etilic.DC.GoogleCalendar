using Etilic.Identity.OAuth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Requests;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Etilic.DC.GoogleCalendar
{
    /// <summary>
    /// Implements the <see cref="Etilic.Identity.OAuth.IOAuthFlow"/> interface for Google Calendar.
    /// </summary>
    public class GoogleCalendarAuthFlow : IOAuthFlow
    {
        #region Instance members
        /// <summary>
        /// The Google Calendar bundle this OAuth flow belongs to.
        /// </summary>
        private GoogleCalendarBundle bundle;
        /// <summary>
        /// The configuration of the OAuth flow.
        /// </summary>
        private IAuthorizationCodeFlow flow;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the globally unique ID of the service this OAuth flow is for.
        /// </summary>
        public Guid ServiceID
        {
            get { return GoogleCalendarBundle.ID; }
        }

        public IAuthorizationCodeFlow Flow
        {
            get { return this.flow; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initialises a new <see cref="Etilic.DC.GoogleCalendar.GoogleCalendarAuthFlow"/> object.
        /// </summary>
        /// <param name="bundle">
        /// The <see cref="Etilic.DC.GoogleCalendar.GoogleCalendarBundle"/> which created this object.
        /// </param>
        public GoogleCalendarAuthFlow(GoogleCalendarBundle bundle)
        {
            // bundle can't be null
            if (bundle == null)
                throw new ArgumentNullException("bundle");

            this.bundle = bundle;

            // configure the OAuth flow
            this.flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets 
                {
                    ClientId = this.bundle.Configuration["GoogleClientID"],
                    ClientSecret = this.bundle.Configuration["GoogleClientSecret"]
                },
                Scopes = new[] { CalendarService.Scope.Calendar }
            });
        }
        #endregion

        #region GetRequestUri
        /// <summary>
        /// 
        /// </summary>
        /// <param name="immediateCallback"></param>
        /// <param name="callbackParam"></param>
        /// <returns></returns>
        public Uri GetRequestUri(String immediateCallback, Guid callbackParam)
        {
            // create an OAuth request
            AuthorizationCodeRequestUrl request = this.flow.CreateAuthorizationCodeRequest(immediateCallback);

            // return the Uri for the request
            return request.Build();
        }
        #endregion

        #region ExchangeCode
        /// <summary>
        /// Exchanges an authentication code for an authentication token.
        /// </summary>
        /// <param name="previousCallback"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public OAuthToken ExchangeCode(String previousCallback, String userID, String code)
        {
            Task<TokenResponse> response = Task.Run<TokenResponse>(async () =>
            {
                return await this.flow.ExchangeCodeForTokenAsync(
                    userID, 
                    code, 
                    previousCallback, 
                    CancellationToken.None);
            });
            response.Wait();

            TokenResponse result = response.Result;

            OAuthToken token = new OAuthToken(this.ServiceID);
            token.AccessToken = result.AccessToken;
            token.RefreshToken = result.RefreshToken;
            token.Scope = result.Scope;
            token.Issued = DateTime.UtcNow;
            token.Expires = token.Issued.AddSeconds((double)result.ExpiresInSeconds);

            // return the OAuth token
            return token;
        }
        #endregion
    }
}
