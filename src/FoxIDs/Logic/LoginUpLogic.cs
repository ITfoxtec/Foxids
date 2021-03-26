﻿using FoxIDs.Infrastructure;
using FoxIDs.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Claims;
using FoxIDs.Models.Logic;
using FoxIDs.Models.Sequences;

namespace FoxIDs.Logic
{
    public class LoginUpLogic : LogicBase
    {
        private readonly TelemetryScopedLogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly SequenceLogic sequenceLogic;

        public LoginUpLogic(TelemetryScopedLogger logger, IServiceProvider serviceProvider, SequenceLogic sequenceLogic, ClaimTransformationsLogic claimTransformationsLogic, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.sequenceLogic = sequenceLogic;
        }

        public async Task<IActionResult> LoginRedirectAsync(UpPartyLink partyLink, LoginRequest loginRequest)
        {
            logger.ScopeTrace("Up, Login redirect.");
            var partyId = await UpParty.IdFormatAsync(RouteBinding, partyLink.Name);
            logger.SetScopeProperty("upPartyId", partyId);

            await loginRequest.ValidateObjectAsync();

            await sequenceLogic.SetUiUpPartyIdAsync(partyId);
            await sequenceLogic.SaveSequenceDataAsync(new LoginUpSequenceData
            {
                DownPartyId = loginRequest.DownParty.Id,
                DownPartyType = loginRequest.DownParty.Type,
                UpPartyId = partyId,
                LoginAction = loginRequest.LoginAction,
                UserId = loginRequest.UserId,
                MaxAge = loginRequest.MaxAge,
                Email = loginRequest.EmailHint,
            });

            return HttpContext.GetUpPartyUrl(partyLink.Name, Constants.Routes.LoginController, includeSequence: true).ToRedirectResult();
        }

        public async Task<IActionResult> LoginResponseAsync(List<Claim> claims)
        {
            logger.ScopeTrace("Up, Login response.");

            var sequenceData = await sequenceLogic.GetSequenceDataAsync<LoginUpSequenceData>();
            logger.SetScopeProperty("upPartyId", sequenceData.UpPartyId);

            logger.ScopeTrace($"Response, Down type {sequenceData.DownPartyType}.");
            switch (sequenceData.DownPartyType)
            {
                case PartyTypes.OAuth2:
                    throw new NotImplementedException();
                case PartyTypes.Oidc:
                    return await serviceProvider.GetService<OidcAuthDownLogic<OidcDownParty, OidcDownClient, OidcDownScope, OidcDownClaim>>().AuthenticationResponseAsync(sequenceData.DownPartyId, claims);
                case PartyTypes.Saml2:
                    claims.AddClaim(Constants.JwtClaimTypes.SubFormat, NameIdentifierFormats.Email.OriginalString);
                    return await serviceProvider.GetService<SamlAuthnDownLogic>().AuthnResponseAsync(sequenceData.DownPartyId, jwtClaims: claims);

                default:
                    throw new NotSupportedException();
            }
        }

        public async Task<IActionResult> LoginResponseErrorAsync(LoginSequenceError error, string errorDescription = null)
        {
            logger.ScopeTrace("Login error response.");

            var sequenceData = await sequenceLogic.GetSequenceDataAsync<LoginUpSequenceData>();
            logger.SetScopeProperty("upPartyId", sequenceData.UpPartyId);

            logger.ScopeTrace($"Response, Down type '{sequenceData.DownPartyType}'.");
            switch (sequenceData.DownPartyType)
            {
                case PartyTypes.OAuth2:
                    throw new NotImplementedException();
                case PartyTypes.Oidc:
                    return await serviceProvider.GetService<OidcAuthDownLogic<OidcDownParty, OidcDownClient, OidcDownScope, OidcDownClaim>>().AuthenticationResponseErrorAsync(sequenceData.DownPartyId, ErrorToOAuth2OidcString(error), errorDescription);
                case PartyTypes.Saml2:
                    return await serviceProvider.GetService<SamlAuthnDownLogic>().AuthnResponseAsync(sequenceData.DownPartyId, status: ErrorToSamlStatus(error));

                default:
                    throw new NotSupportedException($"Party type '{sequenceData.DownPartyType}' not supported.");
            }
        }

        private string ErrorToOAuth2OidcString(LoginSequenceError error)
        {
            switch (error)
            {
                // Default
                case LoginSequenceError.LoginCanceled:
                    return Constants.OAuth.ResponseErrors.LoginCanceled;

                // OAuth


                // Oidc
                case LoginSequenceError.LoginRequired:
                    return IdentityConstants.ResponseErrors.LoginRequired;
                default:
                    throw new NotImplementedException();
            }
        }

        private Saml2StatusCodes ErrorToSamlStatus(LoginSequenceError error)
        {
            switch (error)
            {
                case LoginSequenceError.LoginCanceled:
                    return Saml2StatusCodes.AuthnFailed;

                case LoginSequenceError.LoginRequired:
                    return Saml2StatusCodes.NoAuthnContext;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
