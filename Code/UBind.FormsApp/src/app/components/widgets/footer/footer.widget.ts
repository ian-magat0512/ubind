import { Component, OnInit } from "@angular/core";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { ContentDefinition } from "@app/models/configuration/workflow-step";
import { ApplicationService } from "@app/services/application.service";
import { ConfigService } from "@app/services/config.service";
import { EventService } from "@app/services/event.service";
import { WorkflowService } from "@app/services/workflow.service";
import { takeUntil } from "rxjs/operators";
import { ContentListWidget } from "../content-list/content-list.widget";

/**
 * The footer of the page.
 */
@Component({
    selector: 'ubind-footer-widget',
    templateUrl: './footer.widget.html',
    styleUrls: ['./footer.widget.scss'],
})
export class FooterWidget extends ContentListWidget implements OnInit {

    private currentWorkflowStepName: string;

    public constructor(
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected expressionDependencies: ExpressionDependencies,
        public applicationService: ApplicationService,
        private eventService: EventService,
    ) {
        super(workflowService, configService, expressionDependencies, applicationService);
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.generate();
        this.listenForNextStep();
    }

    protected getContentDefinitions(): Array<ContentDefinition> {
        if (this.configService.workflowSteps[this.currentWorkflowStepName]) {
            return this.configService.workflowSteps[this.currentWorkflowStepName].footer;
        } else {
            // TODO: When we define footer elements at the top level (above the workflow step)
            // then we can grab them here.
            return new Array<ContentDefinition>();
        }
    }

    private generate(): void {
        if (!this.workflowService.currentDestination
            || this.currentWorkflowStepName == this.workflowService.currentDestination.stepName
        ) {
            return;
        }
        this.currentWorkflowStepName = this.workflowService.currentDestination.stepName;
        this.generateContentElements();
    }

    protected listenForNextStep(): void {
        this.eventService.readyForNextStep$.pipe(takeUntil(this.destroyed)).subscribe(() => {
            this.generate();
        });
    }
}
