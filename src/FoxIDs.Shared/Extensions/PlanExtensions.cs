﻿using FoxIDs.Models;

namespace FoxIDs
{
    public static class PlanExtensions
    {
        /// <summary>
        /// Get the default supported key type.
        /// </summary>
        public static TrackKeyTypes GetKeyType(this Plan plan)
        {
            if (plan != null && !plan.EnableKeyVault)
            {
                return TrackKeyTypes.Contained;
            }
            return TrackKeyTypes.KeyVaultRenewSelfSigned;
        }
    }
}
