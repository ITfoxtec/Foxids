﻿using FoxIDs.Infrastructure.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models
{
    public class OAuthDownScope : OAuthDownScope<OAuthDownClaim> { }
    public class OAuthDownScope<TClaim> where TClaim : OAuthDownClaim
    {
        [Required]
        [MaxLength(Constants.Models.OAuthParty.ScopesLength)]
        [RegularExpression(Constants.Models.OAuthParty.ScopeRegExPattern)]
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [Length(Constants.Models.OAuthParty.Client.VoluntaryClaimsMin, Constants.Models.OAuthParty.Client.VoluntaryClaimsMax)]
        [JsonProperty(PropertyName = "voluntary_claims")]
        public List<TClaim> VoluntaryClaims { get; set; }
    }
}
