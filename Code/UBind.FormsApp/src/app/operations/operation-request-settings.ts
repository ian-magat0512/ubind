
export interface OperationRequestSettings {
    /**
     * The number of times the operations should be retried if if fails
     */
    retryAttempts?: number;

    /**
     * The amount of time to wait after a failure before retrying
     */
    retryIntervalMillis?: number;

    /**
     * The amount to multiple the retry interval each time there is a failure
     */
    retryIntervalMultiplier?: number;

    /**
     * A value indicating whether requests should time out and be considered failed if they take too long.
     */
    timeoutRequests?: boolean;

    /**
     * The timeout, after which a follow up attempt may occur
     */
    defaultTimeoutMillis?: number;
}
