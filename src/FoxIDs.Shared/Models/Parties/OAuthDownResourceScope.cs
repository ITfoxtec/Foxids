﻿using FoxIDs.Infrastructure.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models
{
    public class OAuthDownResourceScope
    {
        [Required]
        [MaxLength(Constants.Models.OAuthParty.Client.ResourceScopeLength)]
        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        [Length(Constants.Models.OAuthParty.Client.ScopesMin, Constants.Models.OAuthParty.Client.ScopesMax, Constants.Models.OAuthParty.ScopesLength)]
        [JsonProperty(PropertyName = "scopes")]
        public List<string> Scopes { get; set; }
    }
}
