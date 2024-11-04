﻿using FoxIDs.Infrastructure;
using FoxIDs.Models;
using Api = FoxIDs.Models.Api;
using FoxIDs.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AutoMapper;
using FoxIDs.Infrastructure.Filters;
using FoxIDs.Infrastructure.Security;
using System.Collections.Generic;

namespace FoxIDs.Controllers
{
    [RequireMasterTenant]
    [MasterScopeAuthorize]
    public class MUsageSettingsController : ApiController
    {
        private readonly TelemetryScopedLogger logger;
        private readonly IMapper mapper;
        private readonly IMasterDataRepository masterDataRepository;

        public MUsageSettingsController(TelemetryScopedLogger logger, IMapper mapper, IMasterDataRepository masterDataRepository) : base(logger)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.masterDataRepository = masterDataRepository;
        }

        /// <summary>
        /// Get usage settings.
        /// </summary>
        /// <returns>Usage settings.</returns>
        [ProducesResponseType(typeof(Api.UsageSettings), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Api.UsageSettings>> GetPlan()
        {
            try
            {
                var mUsageSettings = await masterDataRepository.GetAsync<UsageSettings>(await UsageSettings.IdFormatAsync());

                return Ok(mapper.Map<Api.UsageSettings>(mUsageSettings));
            }
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Get '{typeof(Api.UsageSettings).Name}'.");
                    return NotFound(typeof(Api.UsageSettings).Name, "default");
                }
                throw;
            }
        }

        /// <summary>
        /// Update usage settings.
        /// </summary>
        /// <param name="usageSettings">Usage settings.</param>
        /// <returns>UsageSettings.</returns>
        [ProducesResponseType(typeof(Api.UsageSettings), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Api.UsageSettings>> PutPlan([FromBody] Api.UsageSettings usageSettings)
        {
            try
            {
                if (!await ModelState.TryValidateObjectAsync(usageSettings)) return BadRequest(ModelState);

                var mUsageSettings = await masterDataRepository.GetAsync<UsageSettings>(await UsageSettings.IdFormatAsync());
                mUsageSettings.CurrencyExchanges = mapper.Map<List<UsageCurrencyExchange>>(usageSettings.CurrencyExchanges);
                mUsageSettings.InvoiceNumber = usageSettings.InvoiceNumber;
                mUsageSettings.InvoiceNumberPrefix = usageSettings.InvoiceNumberPrefix;
                await masterDataRepository.UpdateAsync(mUsageSettings);

                return Ok(mapper.Map<Api.UsageSettings>(mUsageSettings));
            }
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Update '{typeof(Api.UsageSettings).Name}'.");
                    return NotFound(typeof(Api.UsageSettings).Name, "default");
                }
                throw;
            }
        }
    }
}
