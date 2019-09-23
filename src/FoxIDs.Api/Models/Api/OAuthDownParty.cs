﻿using FoxIDs.Infrastructure.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models.Api
{
    public class OAuthDownParty : IValidatableObject, INameValue
    {
        [MaxLength(Constants.Models.OAuthParty.NameLength)]
        [RegularExpression(Constants.Models.OAuthParty.NameRegExPattern)]
        public string Name { get; set; }

        [Length(Constants.Models.OAuthParty.AllowUpPartyNamesMin, Constants.Models.OAuthParty.AllowUpPartyNamesMax, Constants.Models.OAuthParty.NameLength, Constants.Models.OAuthParty.NameRegExPattern)]
        public List<string> AllowUpPartyNames { get; set; }

        /// <summary>
        /// OAuth 2.0 down client.
        /// </summary>
        [ValidateObject]
        public OAuthDownClient Client { get; set; }

        /// <summary>
        /// OAuth 2.0 down resource.
        /// </summary>
        [ValidateObject]
        public OAuthDownResource Resource { get; set; }

        /// <summary>
        /// Allow cors origins.
        /// </summary>
        [Length(Constants.Models.OAuthParty.AllowCorsOriginsMin, Constants.Models.OAuthParty.AllowCorsOriginsMax, Constants.Models.OAuthParty.AllowCorsOriginLength)]
        public List<string> AllowCorsOrigins { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();
            if(Client == null && Resource == null)
            {
                results.Add(new ValidationResult($"Either the field {nameof(Client)} or the field {nameof(Resource)} is required.", new[] { nameof(Client), nameof(Resource) }));
            }
            return results;
        }
    }
}
