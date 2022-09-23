﻿using FoxIDs.Infrastructure.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace FoxIDs.Models
{
    public class Plan : MasterDocument
    {
        public static async Task<string> IdFormatAsync(IdKey idKey)
        {
            if (idKey == null) new ArgumentNullException(nameof(idKey));
            await idKey.ValidateObjectAsync();

            return $"plan:{idKey.Master}:{idKey.PlanName}";
        }

        public static async Task<string> IdFormatAsync(string planName)
        {
            if (planName == null) new ArgumentNullException(nameof(planName));

            var idKey = new IdKey
            {
                PlanName = planName
            };

            return await IdFormatAsync(idKey);
        }

        public static new string PartitionIdFormat(MasterDocument.IdKey idKey) => $"{idKey.Master}:plan";

        [MaxLength(Constants.Models.Plan.IdLength)]
        [RegularExpression(Constants.Models.Plan.IdRegExPattern)]
        [JsonProperty(PropertyName = "id")]
        public override string Id { get; set; }

        [Required]
        [MaxLength(Constants.Models.Plan.NameLength)]
        [RegularExpression(Constants.Models.Plan.NameRegExPattern)]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [MaxLength(Constants.Models.Plan.TextLength)]
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [Required]
        [MaxLength(Constants.Models.Plan.CurrencyLength)]
        [RegularExpression(Constants.Models.Plan.CurrencyRegExPattern)]
        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [Required]
        [Min(Constants.Models.Plan.CostPerMonthMin)]
        [JsonProperty(PropertyName = "cost_per_month")]
        public decimal CostPerMonth { get; set; }

        [JsonProperty(PropertyName = "enable_custom_domain")]
        public bool EnableCustomDomain { get; set; }

        [Required]
        [JsonProperty(PropertyName = "users")]
        public PlanItem Users { get; set; }

        [Required]
        [JsonProperty(PropertyName = "logins")]
        public PlanItem Logins { get; set; }

        [Required]
        [JsonProperty(PropertyName = "token_req")]
        public PlanItem TokenRequests { get; set; }

        [Required]
        [JsonProperty(PropertyName = "control_api_get_req")]
        public PlanItem ControlApiGetRequests { get; set; }

        [Required]
        [JsonProperty(PropertyName = "control_api_upd_req")]
        public PlanItem ControlApiUpdateRequests { get; set; }

        [MaxLength(Constants.Models.Plan.AppInsightsKeyLength)]
        [RegularExpression(Constants.Models.Plan.AppInsightsKeyRegExPattern)]
        [JsonProperty(PropertyName = "app_ins_key")]
        public string AppInsightsKey { get; set; }

        [MaxLength(Constants.Models.Plan.AppInsightsWorkspaceIdLength)]
        [RegularExpression(Constants.Models.Plan.AppInsightsWorkspaceIdRegExPattern)]
        [JsonProperty(PropertyName = "app_ins_works_id")]
        public string AppInsightsWorkspaceId { get; set; }

        public new class IdKey : MasterDocument.IdKey
        {
            [Required]
            [MaxLength(Constants.Models.Plan.NameLength)]
            [RegularExpression(Constants.Models.Plan.NameRegExPattern)]
            public string PlanName { get; set; }
        }
    }
}
