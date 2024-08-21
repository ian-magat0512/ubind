import { NumberPoolAddResultModel } from "./number-pool-add-result.model";
import { NumberPoolDeleteResultModel } from "./number-pool-delete-result.model";

/**
 * Export number pool update result Resource Model class.
 * This class is the updating of number pool result Resource Model.
 */
export class NumberPoolUpdateResultModel {
    public numberPoolAddResults: Array<NumberPoolAddResultModel> = [];
    public numberPoolDeleteResults: Array<NumberPoolDeleteResultModel> = [];
}
