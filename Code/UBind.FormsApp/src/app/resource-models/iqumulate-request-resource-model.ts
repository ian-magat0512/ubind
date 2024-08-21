import { IQumulateRequestData } from "./iqumulate-request-data";

/**
 * The resource model containing configuration and data for an IQumulate premium funding request
 */
export class IQumulateRequestResourceModel {
    public baseUrl: string;
    public actionUrl: string;
    public messageOriginUrl: string;
    public quoteReference: string;
    public acceptanceConfirmationField: string;
    public iQumulateRequestData: IQumulateRequestData;
}
