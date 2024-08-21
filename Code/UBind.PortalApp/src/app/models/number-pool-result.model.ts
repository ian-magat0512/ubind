import { DeploymentEnvironment } from './deployment-environment.enum';

/**
 * Export number pool get result Resource Model class.
 * This class is the getter of number pool result Resource Model.
 */
export class NumberPoolGetResultModel {
    public environment: DeploymentEnvironment;
    public numbers: Array<string>;
    public productId: string;
}
