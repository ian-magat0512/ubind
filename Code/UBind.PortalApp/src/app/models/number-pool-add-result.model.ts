import { DeploymentEnvironment } from "./deployment-environment.enum";

/**
 * This class is the model of number pool addResult Resource Model.
 */
export class NumberPoolAddResultModel {
    public addedNumbers: Array<string>;
    public duplicateNumbers: Array<string>;
    public environment: DeploymentEnvironment;
    public numberType: string;
}
