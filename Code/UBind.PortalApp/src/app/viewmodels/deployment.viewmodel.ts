import { DeploymentResourceModel } from '@app/models';

/**
 * Export deployment view model class.
 * TODO: Write a better class header: view model of deployment.
 */
export class DeploymentViewModel {
    public isUpdating: boolean;
    public deployment: DeploymentResourceModel;

    public constructor() {
        this.isUpdating = true;
    }

    public get hasDeployment(): boolean {
        return this.deployment != null;
    }

    public onBeginningDeployment(): void {
        this.deployment = null;
        this.isUpdating = true;
    }

    public updateDeployment(deployment: DeploymentResourceModel): void {
        this.isUpdating = false;
        this.deployment = deployment;
    }
}
