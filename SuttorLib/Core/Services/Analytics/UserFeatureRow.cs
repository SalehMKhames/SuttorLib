namespace SuttorLib.Core.Services.Analytics
{
    /// <summary>
    /// Per-user behavioral features used for clustering, engagement and anomaly detection.
    /// </summary>
    public class UserFeatureRow
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public int Downloads { get; set; }
        public int FinishedReads { get; set; }
        public int Favorites { get; set; }
        public int Ratings { get; set; }
        public int Comments { get; set; }

        public double EngagementScore(double wRead, double wLike, double wRating, double wComment) =>
            Downloads * wRead + Favorites * wLike + Ratings * wRating + Comments * wComment;
    }
}
