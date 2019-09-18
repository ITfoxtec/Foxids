﻿using FoxIDs.Infrastructure.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FoxIDs.Models
{
    public class OAuthDownResource
    {
        [JsonIgnore]
        internal PartyDataElement Parent { private get; set; }

        [JsonIgnore]
        public string ResourceId { get => Parent.Name; }

        [Length(Constants.Models.OAuthParty.Resource.ScopesMin, Constants.Models.OAuthParty.Resource.ScopesMax, Constants.Models.OAuthParty.ScopesLength)]
        [JsonProperty(PropertyName = "scopes")]
        public List<string> Scopes { get; set; }
    }
}
