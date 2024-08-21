import { DeploymentEnvironment } from "./deployment-environment.enum";

/**
 * Represent a change to the data environment (e.g. development, staging, production)
 */
export interface EnvironmentChange {
    oldEnvironment: DeploymentEnvironment;
    newEnvironment: DeploymentEnvironment;
}
