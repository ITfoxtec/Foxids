﻿using FoxIDs.Infrastructure;
using FoxIDs.Models;
using FoxIDs.Models.Sequences;
using FoxIDs.Repository;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FoxIDs.Logic
{
    public class ExternalUserLogic : LogicBase
    {
        private readonly TelemetryScopedLogger logger;
        private readonly ITenantDataRepository tenantDataRepository;
        private readonly SequenceLogic sequenceLogic;
        private readonly ClaimsDownLogic claimsDownLogic;
        private readonly ClaimTransformLogic claimTransformLogic;

        public ExternalUserLogic(TelemetryScopedLogger logger, ITenantDataRepository tenantDataRepository, SequenceLogic sequenceLogic, ClaimsDownLogic claimsDownLogic, ClaimTransformLogic claimTransformLogic, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this.logger = logger;
            this.tenantDataRepository = tenantDataRepository;
            this.sequenceLogic = sequenceLogic;
            this.claimsDownLogic = claimsDownLogic;
            this.claimTransformLogic = claimTransformLogic;
        }


        public async Task<(IActionResult externalUserActionResult, IEnumerable<Claim> externalUserClaims)> HandleUserAsync(UpPartyExternal<UpPartyProfile> party, IEnumerable<Claim> claims, Action<ExternalUserUpSequenceData> populateSequenceDataAction, Action<string> requireUserExceptionAction)
        {
            if (string.IsNullOrWhiteSpace(party.LinkExternalUser?.LinkClaimType))
            {
                return (null, null);
            }

            var linkClaimType = party.LinkExternalUser.LinkClaimType;
            if (party.Type == PartyTypes.Saml2)
            {
                var jwtLinkClaimTypes = claimsDownLogic.FromSamlToJwtInfoClaimType(linkClaimType);
                if (jwtLinkClaimTypes.Count() > 0)
                {
                    linkClaimType = jwtLinkClaimTypes.First();
                }
            }

            var linkClaimValue = GetLinkClaim(linkClaimType, claims);
            logger.ScopeTrace(() => $"Validating external user, link claim type '{party.LinkExternalUser.LinkClaimType}' and value '{linkClaimValue}', Route '{RouteBinding?.Route}'.");
            if (!linkClaimValue.IsNullOrWhiteSpace())
            {
                var externalUser = await tenantDataRepository.GetAsync<ExternalUser>(await ExternalUser.IdFormatAsync(RouteBinding, party.Name, await linkClaimValue.HashIdStringAsync()), required: false);
                if (externalUser != null)
                {
                    if (!externalUser.DisableAccount)
                    {
                        var externalUserClaims = GetExternalUserClaim(party, externalUser);
                        logger.ScopeTrace(() => $"AuthMethod, External user output JWT claims '{externalUserClaims.ToFormattedString()}'", traceType: TraceTypes.Claim);
                        return (null, externalUserClaims);
                    }
                }
                else if (party.LinkExternalUser.AutoCreateUser)
                {
                    if (party.LinkExternalUser.Elements?.Count > 0)
                    {
                        return (await StartUICreateUserAsync(party, linkClaimValue, claims, populateSequenceDataAction), null);
                    }
                    else
                    {
                        return (null, await CreateUserAsync(party, linkClaimValue));
                    }
                }

                if (party.LinkExternalUser.RequireUser)
                {
                    requireUserExceptionAction($"Require external user for link claim type '{party.LinkExternalUser.LinkClaimType}' and value '{linkClaimValue}'.");
                }
            }
            else
            {
                try
                {
                    throw new EndpointException($"External user, link claim value is empty for link claim type '{party.LinkExternalUser.LinkClaimType}'.") { RouteBinding = RouteBinding };
                }
                catch (Exception ex)
                {
                    logger.Warning(ex);
                }            
            }

            return (null, null);
        }

        public async Task<IEnumerable<Claim>> CreateUserAsync(UpPartyExternal<UpPartyProfile> upParty, string linkClaimValue, IEnumerable<Claim> dynamicElementClaims = null)
        {
            logger.ScopeTrace(() => $"Creating external user, link claim value '{linkClaimValue}', Route '{RouteBinding?.Route}'.");

            dynamicElementClaims = dynamicElementClaims ?? new List<Claim>();
            var transformedClaims = await claimTransformLogic.Transform(upParty.LinkExternalUser.ClaimTransforms?.ConvertAll(t => (ClaimTransform)t), dynamicElementClaims);

            var externalUser = new ExternalUser
            {
                Id = await ExternalUser.IdFormatAsync(RouteBinding, upParty.Name, await linkClaimValue.HashIdStringAsync()),
                UserId = Guid.NewGuid().ToString(),
                LinkClaimValue = linkClaimValue,
                Claims = transformedClaims.ToClaimAndValues()
            };

            await tenantDataRepository.CreateAsync(externalUser);
            logger.ScopeTrace(() => $"External user created, with user id '{externalUser.UserId}'.");

            var externalUserClaims = GetExternalUserClaim(upParty, externalUser);
            logger.ScopeTrace(() => $"AuthMethod, Created external user output JWT claims '{externalUserClaims.ToFormattedString()}'", traceType: TraceTypes.Claim);
            return externalUserClaims;
        }  

        private async Task<IActionResult> StartUICreateUserAsync(UpPartyExternal<UpPartyProfile> party, string linkClaimValue, IEnumerable<Claim> claims, Action<ExternalUserUpSequenceData> populateSequenceDataAction)
        {
            logger.ScopeTrace(() => $"Start UI create external user, link claim '{linkClaimValue}', Route '{RouteBinding?.Route}'.");
            var sequenceData = new ExternalUserUpSequenceData
            {
                UpPartyId = party.Id,
                UpPartyType = party.Type,
                Claims = claims?.ToClaimAndValues(),
                LinkClaimValue = linkClaimValue
            };
            populateSequenceDataAction(sequenceData);
            await sequenceLogic.SaveSequenceDataAsync(sequenceData);
            return HttpContext.GetUpPartyUrl(party.Name, Constants.Routes.ExtController, Constants.Endpoints.CreateUser, includeSequence: true).ToRedirectResult(RouteBinding.DisplayName);
        }

        private List<Claim> GetExternalUserClaim(UpPartyExternal<UpPartyProfile> party, ExternalUser externalUser)
        {
            var claims = externalUser.Claims?.ToClaimList() ?? new List<Claim>();
            var userIdClaims = claims.Where(c => c.Type == Constants.JwtClaimTypes.LocalSub).Select(c => c.Value);
            if (userIdClaims.Count() > 0)
            {
                claims = claims.Where(c => c.Type != Constants.JwtClaimTypes.LocalSub).ToList();
                foreach (var userIdClaim in userIdClaims)
                {
                    claims.Add(new Claim(Constants.JwtClaimTypes.LocalSub, $"{party.Name}|{userIdClaim}"));
                }
            }

            claims.AddClaim(Constants.JwtClaimTypes.LocalSub, externalUser.UserId);
            return claims;
        }

        private string GetLinkClaim(string linkClaimType, IEnumerable<Claim> claims) => claims.Where(c => c.Type.Equals(linkClaimType, StringComparison.OrdinalIgnoreCase)).Select(c => c.Value).FirstOrDefault();

        public List<Claim> AddExternalUserClaims(UpPartyExternal<UpPartyProfile> party, List<Claim> claims, IEnumerable<Claim> externalUserClaims)
        {
            if (externalUserClaims?.Count() > 0)
            {
                claims = party.LinkExternalUser?.OverwriteClaims == true ? externalUserClaims.ConcatOnce(claims).ToList() : externalUserClaims.Concat(claims).ToList();
            }
            return claims;
        }
    }
}
