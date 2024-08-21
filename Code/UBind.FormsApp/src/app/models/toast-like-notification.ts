/**
 * Represents a notification to be displayed on screen for a specific time period.
 */
export interface ToastLikeNotification {

    message: string;

    /**
     * The number of milliseconds to display the notification for.
     * If not passed, defaults to 3000.
     */
    expireAfterMillis?: number;

    /**
     * If set to true, when the next notification arrives, this notification
     * will automatically expire and hide.
     */
    expireUponNextNotification?: boolean;

    /* ------- The following properties are used for internal tracking purposes ------- */

    expired?: boolean;

    active?: boolean;

    animatingOut?: boolean;

    startTimeMillis?: number;
}
