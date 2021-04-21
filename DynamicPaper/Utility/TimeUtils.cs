namespace Maxstupo.DynamicPaper.Utility {

    using System;

    public static class TimeUtils {

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    
        public static long CurrentTimeMilliseconds => (long) (DateTime.UtcNow - Jan1st1970).TotalMilliseconds;

    }

}