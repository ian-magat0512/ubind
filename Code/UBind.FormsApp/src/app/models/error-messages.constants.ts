import { ErrorCode } from "./error-codes.enum";

/**
 * Export error message class.
 * TODO: Write a better class header: Error messages to be thrown.
 */
export class ErrorMessages {
    public static readonly ApplicationInternalError: string =
        `The server reported an error during the handling of your request. `
        + `Please try again, and if you're still having issues, `
        + `please don't hesitate to contact us for assistance.`;
    public static readonly SlowNetworkConnectionOnCalculation: string =
        `It seems to be taking a long time to calculate a price or amount. `
        + `This could be due to a slow network connection on your end, `
        + `or an issue with one of our integrated providers. `
        + `If it is one of our providers, note that we would already be aware of the issue. `
        + `If you are on a slow network connection please feel free to continue waiting, `
        + `or you may wish to retry your request.`;
    public static readonly ServerInternalError: string =
        `The server reported an error during the handling of your request. `
        + `The issue has been logged and a notification has been sent to our support team. `
        + `In the mean time, please try again, and if you're still having issues, `
        + `please don't hesitate to contact us for assistance.`;
    public static readonly UnableToContactTheServer: string =
        `We were unable to contact the server. `
        + `Please check your internet connection and try again`;
    public static readonly CalculationErrorCodes: Array<ErrorCode> =
        [ErrorCode.Code10042, ErrorCode.Code10043];

}
