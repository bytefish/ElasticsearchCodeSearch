namespace ElasticsearchCodeSearch.Database
{
    /// <summary>
    /// Job Status for a given indexing job.
    /// </summary>
    public enum JobStatusEnum
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Job has been enqueued.
        /// </summary>
        Enqueued = 1,

        /// <summary>
        /// Job has been scheduled.
        /// </summary>
        Scheduled = 2,

        /// <summary>
        /// Job is currently being processed.
        /// </summary>
        Running = 3,
        
        /// <summary>
        /// Job has failed due to an error.
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Job has been canceled.
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// Job has finished successfully.
        /// </summary>
        Completed = 5
    }
}
