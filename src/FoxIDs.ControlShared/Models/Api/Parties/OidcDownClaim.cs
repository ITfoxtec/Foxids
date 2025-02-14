﻿using FoxIDs.Infrastructure.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoxIDs.Models.Api
{
    public class OidcDownClaim
    {
        [Required]
        [MaxLength(Constants.Models.Claim.JwtTypeLength)]
        [RegularExpression(Constants.Models.Claim.JwtTypeWildcardRegExPattern)]
        [Display(Name = "Claim")]
        public string Claim { get; set; }

        [ListLength(Constants.Models.Claim.ValuesOAuthMin, Constants.Models.Claim.ValuesMax, Constants.Models.Claim.ValueLength, Constants.Models.Claim.ValueLength)]
        [Display(Name = "Values")]
        public List<string> Values { get; set; }

        /// <summary>
        /// Claim also in ID token, default false.
        /// </summary>
        [Display(Name = "Include in ID token")]
        public bool? InIdToken { get; set; } = false;
    }
}
