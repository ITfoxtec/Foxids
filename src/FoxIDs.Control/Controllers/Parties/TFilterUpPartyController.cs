﻿using AutoMapper;
using FoxIDs.Infrastructure;
using FoxIDs.Repository;
using FoxIDs.Models;
using Api = FoxIDs.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using ITfoxtec.Identity;
using System;
using FoxIDs.Infrastructure.Security;

namespace FoxIDs.Controllers
{
    [TenantScopeAuthorize(Constants.ControlApi.Segment.Party)]
    public class TFilterUpPartyController : ApiController
    {
        private const string dataType = Constants.Models.DataType.UpParty;
        private readonly TelemetryScopedLogger logger;
        private readonly IMapper mapper;
        private readonly ITenantDataRepository tenantRepository;

        public TFilterUpPartyController(TelemetryScopedLogger logger, IMapper mapper, ITenantDataRepository tenantRepository) : base(logger)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.tenantRepository = tenantRepository;
        }

        /// <summary>
        /// Filter authentication method.
        /// </summary>
        /// <param name="filterName">Filter authentication method name.</param>
        /// <returns>Authentication methods.</returns>
        [ProducesResponseType(typeof(HashSet<Api.UpParty>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HashSet<Api.UpParty>>> GetFilterUpParty(string filterName)
        {
            try
            {
                var doFilterPartyType = Enum.TryParse<PartyTypes>(filterName, out var filterPartyType);
                var idKey = new Track.IdKey { TenantName = RouteBinding.TenantName, TrackName = RouteBinding.TrackName };
                (var mUpPartys, _) = filterName.IsNullOrWhiteSpace() ? 
                    await tenantRepository.GetListAsync<UpParty>(idKey, whereQuery: p => p.DataType.Equals(dataType)) : 
                    await tenantRepository.GetListAsync<UpParty>(idKey, whereQuery: p => p.DataType.Equals(dataType) && 
                        (p.Name.Contains(filterName, StringComparison.OrdinalIgnoreCase) || p.DisplayName.Contains(filterName, StringComparison.OrdinalIgnoreCase) || (doFilterPartyType && p.Type == filterPartyType)));
             
                var aUpPartys = new HashSet<Api.UpParty>(mUpPartys.Count());
                foreach(var mUpParty in mUpPartys.OrderBy(p => p.Type).ThenBy(p => p.Name))
                {
                    aUpPartys.Add(mapper.Map<Api.UpParty>(mUpParty));
                }
                return Ok(aUpPartys);
            }
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Get '{typeof(Api.UpParty).Name}' by filter name '{filterName}'.");
                    return NotFound(typeof(Api.UpParty).Name, filterName);
                }
                throw;
            }
        }
    }
}
