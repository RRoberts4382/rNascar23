﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using rNascar23.Data;
using rNascar23.Points.Models;
using rNascar23.Points.Ports;
using rNascar23.Service.Points.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rNascar23.Service.Points.Adapters
{
    internal class PointsRepository : JsonDataRepository, IPointsRepository
    {
        private readonly IMapper _mapper;
        private readonly ILogger<PointsRepository> _logger;

        public string Url { get => @"https://cf.nascar.com/live/feeds/series_{0}/{1}/live_points.json"; }

        public PointsRepository(IMapper mapper, ILogger<PointsRepository> logger)
            : base(logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IList<DriverPoints>> GetDriverPoints(int raceId, int seriesId)
        {
            try
            {
                var absoluteUrl = BuildUrl(seriesId, raceId);

                var json = await GetAsync(absoluteUrl).ConfigureAwait(false);

                if (string.IsNullOrEmpty(json))
                    return new List<DriverPoints>();

                var model = JsonConvert.DeserializeObject<DriverPointsModel[]>(json);

                var liveFeed = _mapper.Map<IList<DriverPoints>>(model);

                return liveFeed;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IList<StagePoints2>> GetStagePoints(int raceId, int seriesId)
        {
            string json = string.Empty;

            try
            {

                var absoluteUrl = BuildUrl(seriesId, raceId);

                json = await GetAsync(absoluteUrl).ConfigureAwait(false);

                var models = JsonConvert.DeserializeObject<StagePointsModel2[]>(json);

                var stagePoints = _mapper.Map<IList<StagePoints2>>(models);

                return stagePoints;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string BuildUrl(int seriesId, int eventId)
        {
            return String.Format(Url, seriesId, eventId);
        }
    }
}
