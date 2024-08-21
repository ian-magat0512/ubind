import { Component } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { WorkingConfiguration } from '@app/models/configuration/working-configuration';
import { EventService } from '@app/services/event.service';

/**
 * This component provides the ability to navigate to an arbitrary step in the workflow.
 */
@Component({
    selector: 'workflow-tools',
    templateUrl: './workflow-tools.component.html',
    styleUrls: ['./workflow-tools.component.scss'],
})
export class WorkflowToolsComponent {
    public output: string;
    public form: FormGroup = new FormGroup({});
    public workflowStepNames: Array<string> = new Array<string>();

    public constructor(
        private configService: ConfigService,
        private workflowService: WorkflowService,
        eventService: EventService,
        formBuilder: FormBuilder,
    ) {
        this.form = formBuilder.group({
        });

        eventService.loadedConfiguration.subscribe((config: WorkingConfiguration) => this.populateWorkflowStepNames());
        eventService.updatedConfiguration.subscribe((config: WorkingConfiguration) => this.populateWorkflowStepNames());
    }

    private populateWorkflowStepNames(): void {
        this.workflowStepNames = Object.getOwnPropertyNames(this.configService.workflowSteps);
    }

    public onWorkflowStepClicked(workflowStepName: string): void {
        this.workflowService.navigateTo({ stepName: workflowStepName });
    }
}
