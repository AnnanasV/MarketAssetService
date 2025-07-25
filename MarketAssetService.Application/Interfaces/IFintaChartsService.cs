﻿using MarketAssetService.Application.DTOs;

namespace MarketAssetService.Application.Interfaces;
public interface IFintaChartsService
{
    Task<List<FintaInstrumentDto>> GetInstrumentsAsync(string provider = "oanda", string kind = "forex");
    Task<HistoricalPriceDto?> GetLatestPriceAsync(Guid instrumentId, string provider = "oanda");
}
