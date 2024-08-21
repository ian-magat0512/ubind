import { ReleaseResourceModel } from './release.resource-model';
import { DeploymentEnvironment } from '../models/deployment-environment.enum';

/**
 * Resource model for the deployment of a release to an environment
 */
export interface DeploymentResourceModel {
    environment: DeploymentEnvironment;
    release: ReleaseResourceModel;
}
