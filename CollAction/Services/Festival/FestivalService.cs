using System;
using Microsoft.Extensions.Options;

namespace CollAction.Services.Festival
{
    public class FestivalService : IFestivalService
    {
        private readonly FestivalServiceOptions _festivalOptions;

        public FestivalService(IOptions<FestivalServiceOptions> festivalOptions)
        {
            _festivalOptions = festivalOptions.Value;
        }

        public bool CallToActionsVisible => _festivalOptions.FestivalEndDate.HasValue && _festivalOptions.FestivalEndDate >= DateTime.Now;
    }
}