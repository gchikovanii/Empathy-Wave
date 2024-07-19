﻿namespace EmphatyWave.ApiService.Infrastructure.Extensions
{
    public static class CacheDurationExtensions
    {
        public static int ToSeconds(this TimeSpan timeSpan)
        {
            return (int)timeSpan.TotalSeconds;
        }
    }
}