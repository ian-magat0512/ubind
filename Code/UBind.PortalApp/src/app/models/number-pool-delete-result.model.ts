import { DeploymentEnvironment } from "./deployment-environment.enum";

/**
 * Export number pool delete result Resource Model class.
 * This class is the deletion of number pool result Resource Model.
 */
export class NumberPoolDeleteResultModel {
    public deletedNumbers: Array<string>;
    public environment: DeploymentEnvironment;
    public numberType: string;
}
