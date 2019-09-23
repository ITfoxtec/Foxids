﻿using FoxIDs.Infrastructure;
using FoxIDs.Models;
using Api = FoxIDs.Models.Api;
using FoxIDs.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;

namespace FoxIDs.Controllers
{
    /// <summary>
    /// OAuth 2.0 down party api.
    /// </summary>
    public class TOAuthDownPartyController : TenantApiController
    {
        private readonly TelemetryScopedLogger logger;
        private readonly IMapper mapper;
        private readonly ITenantRepository tenantService;

        public TOAuthDownPartyController(TelemetryScopedLogger logger, IMapper mapper, ITenantRepository tenantService) : base(logger)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.tenantService = tenantService;
        }

        /// <summary>
        /// Get OAuth 2.0 down party.
        /// </summary>
        /// <param name="name">Party id.</param>
        /// <returns>OAuth 2.0 down party.</returns>
        [ProducesResponseType(typeof(Api.OAuthDownParty), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Api.OAuthDownParty>> Get(string name)
        {
            try
            {
                if (!ModelState.TryValidateParameterAsync(name, nameof(name))) return BadRequest(ModelState);

                var oauthDownParty = await tenantService.GetAsync<OAuthDownParty>(await DownParty.IdFormat(RouteBinding, name));
                return Ok(mapper.Map<Api.OAuthDownParty>(oauthDownParty));
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.Warning(ex, $"Get by name '{name}'.");
                    return NotFound(nameof(Api.OAuthDownParty), name);
                }
                throw;
            }
        }

        /// <summary>
        /// Create OAuth 2.0 down party.
        /// </summary>
        /// <param name="response">OAuth 2.0 down party.</param>
        /// <returns>OAuth 2.0 down party.</returns>
        [ProducesResponseType(typeof(Api.OAuthDownParty), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Api.OAuthDownParty>> Post([FromBody] Api.OAuthDownParty response)
        {
            try
            {
                if (!await ModelState.TryValidateObjectAsync(response)) return BadRequest(ModelState);

                var oauthDownParty = mapper.Map<OAuthDownParty>(response);
                await tenantService.CreateAsync(oauthDownParty);

                return Created(mapper.Map<Api.OAuthDownParty>(oauthDownParty));
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    logger.Warning(ex, $"Create by name '{response.Name}'.");
                    return Conflict(nameof(Api.OAuthDownParty), response.Name);
                }
                throw;
            }
        }

        /// <summary>
        /// Update OAuth 2.0 down party.
        /// </summary>
        /// <param name="response">OAuth 2.0 down party.</param>
        /// <returns>OAuth 2.0 down party.</returns>
        [ProducesResponseType(typeof(Api.OAuthDownParty), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Api.OAuthDownParty>> Put([FromBody] Api.OAuthDownParty response)
        {
            try
            {
                if (!await ModelState.TryValidateObjectAsync(response)) return BadRequest(ModelState);

                var oauthDownParty = mapper.Map<OAuthDownParty>(response);
                await tenantService.UpdateAsync(oauthDownParty);

                return Ok(mapper.Map<Api.OAuthDownParty>(oauthDownParty));
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.Warning(ex, $"Update by name '{response.Name}'.");
                    return NotFound(nameof(Api.OAuthDownParty), response.Name);
                }
                throw;
            }
        }

        /// <summary>
        /// Delete OAuth 2.0 down party.
        /// </summary>
        /// <param name="name">Party id.</param>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string name)
        {
            try
            {
                if (!ModelState.TryValidateParameterAsync(name, nameof(name))) return BadRequest(ModelState);

                await tenantService.DeleteAsync<OAuthDownParty>(await DownParty.IdFormat(RouteBinding, name));
                return NoContent();
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.Warning(ex, $"Delete by id '{name}'.");
                    return NotFound(nameof(Api.OAuthDownParty), name);
                }
                throw;
            }
        }
    }
}
