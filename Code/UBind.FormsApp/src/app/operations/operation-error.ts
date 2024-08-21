/**
 * Export Operation Error class.
 * Extension of Error class with title header and details.
 * @dynamic
 */
export class OperationError extends Error {
    public title: string;
    public additionalDetailsTitle: string;
    public additionalDetails: Array<string>;

    public constructor(
        title: string,
        message: string,
        additionalDetails?: Array<string>,
        additionalDetailsTitle?: string,
    ) {
        super(message);
        this.name = OperationError.name;
        this.title = title ? title : "Operation Error";
        this.additionalDetails = additionalDetails;
        this.additionalDetailsTitle = additionalDetailsTitle;
    }
}
