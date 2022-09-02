﻿using FoxIDs.Client.Shared.Components;
using FoxIDs.Models.Api;

namespace FoxIDs.Client.Models.ViewModels
{
    public class GeneralTenantViewModel : Tenant
    {
        public GeneralTenantViewModel()
        { }

        public GeneralTenantViewModel(Tenant tenant)
        {
            Name = tenant.Name;
            CustomDomain = tenant.CustomDomain;
            CustomDomainVerified = tenant.CustomDomainVerified;
        }

        public bool Edit { get; set; }

        public bool ShowAdvanced { get; set; }

        public bool DeleteAcknowledge { get; set; }

        public string Error { get; set; }

        public string LoginUri { get; set; }

        public PageEditForm<Tenant> Form { get; set; }
    }
}
