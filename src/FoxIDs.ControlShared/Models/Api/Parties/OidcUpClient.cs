﻿using FoxIDs.Infrastructure.DataAnnotations;
using ITfoxtec.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models.Api
{
    public class OidcUpClient : IValidatableObject
    {
        [MaxLength(Constants.Models.OAuthUpParty.Client.ClientIdLength)]
        [Display(Name = "Optional custom SP client ID (default the party name)")]
        public string SpClientId { get; set; }

        [Length(Constants.Models.OAuthUpParty.Client.ScopesMin, Constants.Models.OAuthUpParty.Client.ScopesMax, Constants.Models.OAuthUpParty.ScopeLength, Constants.Models.OAuthUpParty.ScopeRegExPattern)]
        [Display(Name = "Scopes")]
        public List<string> Scopes { get; set; }

        [Length(Constants.Models.OAuthUpParty.Client.ClaimsMin, Constants.Models.OAuthUpParty.Client.ClaimsMax, Constants.Models.Claim.JwtTypeLength, Constants.Models.Claim.JwtTypeRegExPattern)]
        [Display(Name = "Claims")]
        public List<string> Claims { get; set; }

        [Required]
        [MaxLength(Constants.Models.OAuthUpParty.Client.ResponseModeLength)]
        [Display(Name = "Response mode")]
        public string ResponseMode { get; set; } = IdentityConstants.ResponseModes.FormPost;

        [Required]
        [MaxLength(Constants.Models.OAuthUpParty.Client.ResponseTypeLength)]
        [Display(Name = "Response type")]
        public string ResponseType { get; set; } = IdentityConstants.ResponseTypes.Code;

        [MaxLength(Constants.Models.OAuthUpParty.Client.AuthorizeUrlLength)]
        [Display(Name = "Authorize URL")]
        public string AuthorizeUrl { get; set; }

        [MaxLength(Constants.Models.OAuthUpParty.Client.TokenUrlLength)]
        [Display(Name = "Token URL")]
        public string TokenUrl { get; set; }

        [MaxLength(Constants.Models.OAuthUpParty.Client.EndSessionUrlLength)]
        [Display(Name = "End session URL")]
        public string EndSessionUrl { get; set; }

        [MaxLength(Constants.Models.SecretHash.SecretLength)]
        [Display(Name = "Client secret")]
        public string ClientSecret { get; set; }

        [Display(Name = "Require PKCE")]
        public bool RequirePkce { get; set; } = true;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if (RequirePkce && ResponseType?.Contains(IdentityConstants.ResponseTypes.Code) != true)
            {
                results.Add(new ValidationResult($"Require '{IdentityConstants.ResponseTypes.Code}' response type with PKCE.", new[] { nameof(RequirePkce) }));
            }
            if (ResponseType?.Contains(IdentityConstants.ResponseTypes.Code) == true)
            {
                if (ClientSecret.IsNullOrEmpty())
                {
                    results.Add(new ValidationResult($"Require '{nameof(ClientSecret)}' to execute '{IdentityConstants.ResponseTypes.Code}' response type.", new[] { nameof(ClientSecret) }));
                }
            }
            if (!(ResponseMode?.Equals(IdentityConstants.ResponseModes.Query) == true || ResponseMode?.Equals(IdentityConstants.ResponseModes.FormPost) == true))
            {
                results.Add(new ValidationResult($"Invalid response mode '{ResponseMode}'. '{IdentityConstants.ResponseModes.FormPost}' and '{IdentityConstants.ResponseModes.Query}' is supported. ", new[] { nameof(ResponseMode) }));
            }
            return results;
        }
    }
}
