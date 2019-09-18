﻿using FoxIDs.Infrastructure.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models
{
    public class OAuthDownClient : OAuthDownClient<OAuthDownScope, OAuthDownClaim> { }
    public class OAuthDownClient<TScope, TClaim> where TScope : OAuthDownScope<TClaim> where TClaim : OAuthDownClaim
    {
        [JsonIgnore]
        internal PartyDataElement Parent { private get; set; }

        [JsonIgnore]
        public string ClientId { get => Parent.Name; }

        [Length(Constants.Models.OAuthParty.Client.ResourceScopesMin, Constants.Models.OAuthParty.Client.ResourceScopesMax)]
        [JsonProperty(PropertyName = "resource_scopes")]
        public List<OAuthDownResourceScope> ResourceScopes { get; set; }

        [Length(Constants.Models.OAuthParty.Client.ScopesMin, Constants.Models.OAuthParty.Client.ScopesMax)]
        [JsonProperty(PropertyName = "scopes")]
        public List<TScope> Scopes { get; set; }

        [Length(Constants.Models.OAuthParty.Client.ClaimsMin, Constants.Models.OAuthParty.Client.ClaimsMax)]
        [JsonProperty(PropertyName = "claims")]
        public List<TClaim> Claims { get; set; }

        [Length(Constants.Models.OAuthParty.Client.ResponseTypesMin, Constants.Models.OAuthParty.Client.ResponseTypesMax, Constants.Models.OAuthParty.Client.ResponseTypeLength)]
        [JsonProperty(PropertyName = "response_types")]
        public List<string> ResponseTypes { get; set; }

        [Length(Constants.Models.OAuthParty.Client.RedirectUrisMin, Constants.Models.OAuthParty.Client.RedirectUrisMax, Constants.Models.OAuthParty.Client.RedirectUriLength)]
        [JsonProperty(PropertyName = "redirect_uris")]
        public List<string> RedirectUris { get; set; }

        [Length(Constants.Models.OAuthParty.Client.SecretsMin, Constants.Models.OAuthParty.Client.SecretsMax)]
        [JsonProperty(PropertyName = "secrets")]
        public List<OAuthClientSecret> Secrets { get; set; }

        [Range(Constants.Models.OAuthParty.Client.AuthorizationCodeLifetimeMin, Constants.Models.OAuthParty.Client.AuthorizationCodeLifetimeMax)] 
        [JsonProperty(PropertyName = "authorization_code_lifetime")]
        public int? AuthorizationCodeLifetime { get; set; }

        [Range(Constants.Models.OAuthParty.Client.AccessTokenLifetimeMin, Constants.Models.OAuthParty.Client.AccessTokenLifetimeMax)]
        [JsonProperty(PropertyName = "access_token_lifetime")]
        public int AccessTokenLifetime { get; set; }

        [Range(Constants.Models.OAuthParty.Client.RefreshTokenLifetimeMin, Constants.Models.OAuthParty.Client.RefreshTokenLifetimeMax)]
        [JsonProperty(PropertyName = "refresh_token_lifetime")]
        public int? RefreshTokenLifetime { get; set; }

        [Range(Constants.Models.OAuthParty.Client.RefreshTokenAbsoluteLifetimeMin, Constants.Models.OAuthParty.Client.RefreshTokenAbsoluteLifetimeMax)]
        [JsonProperty(PropertyName = "refresh_token_absolute_lifetime")]
        public int? RefreshTokenAbsoluteLifetime { get; set; }

        [JsonProperty(PropertyName = "refresh_token_use_one_time")]
        public bool? RefreshTokenUseOneTime { get; set; }

        [JsonProperty(PropertyName = "refresh_token_lifetime_unlimited")]
        public bool? RefreshTokenLifetimeUnlimited { get; set; }        
    }
}
