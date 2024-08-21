import { Injectable } from '@angular/core';
import { DefaultSectionWidgetStatus } from '@app/components/widgets/section/default-section-widget-status';
import { SectionWidgetStatus } from '@app/components/widgets/section/section-widget-status';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { Subject } from 'rxjs';

/**
 * Export workflow status service class.
 * TODO: Write a better class header: workflow status functions.
 */
@Injectable({
    providedIn: 'root',
})
export class WorkflowStatusService {

    public actionInProgressSubject: Subject<boolean> = new Subject<boolean>();
    public navigationInProgressSubject: Subject<boolean> = new Subject<boolean>();
    public quoteLoadedSubject: Subject<boolean> = new Subject<boolean>();
    public loadedCustomerHasUserSubject: Subject<boolean> = new Subject<boolean>();
    public navigatingOutStarted: Subject<void> = new Subject<void>();

    public actionsInProgress: Array<string> = new Array<string>();
    private currentNavigationActionName: string;
    private _workflowStepChangeInProgress: boolean = false;

    public workflowStepHistory: Array<string> = [];
    public isApplicationLoaded: boolean = false;
    private _isNavigatingOut: boolean = false;
    public currentSectionWidgetStatus: SectionWidgetStatus;
    public targetDestination: WorkflowDestination;

    public constructor() {
        this.currentSectionWidgetStatus = new DefaultSectionWidgetStatus();
    }

    public startNavigation(actionName: string): void {
        if (this.currentNavigationActionName != actionName) {
            this.currentNavigationActionName = actionName;
            this.navigationInProgressSubject.next(true);
        }
    }

    public stopNavigation(): void {
        this.currentNavigationActionName = null;
        this._workflowStepChangeInProgress = false;
        this.navigationInProgressSubject.next(false);
    }

    public setWorkflowStepChangeInProgress(workflowStepChangeInProgress: boolean): void {
        this._workflowStepChangeInProgress = workflowStepChangeInProgress;
    }

    public isNavigationInProgress(actionName?: string): boolean {
        if (actionName) {
            return this.currentNavigationActionName == actionName;
        } else {
            return this.currentNavigationActionName != null;
        }
    }

    public get workflowStepChangeInProgress(): boolean {
        return this._workflowStepChangeInProgress;
    }

    public set isNavigatingOut(isNavigatingOut: boolean) {
        this._isNavigatingOut = isNavigatingOut;
        if (isNavigatingOut) {
            this.navigatingOutStarted.next();
        }
    }

    public get isNavigatingOut(): boolean {
        return this._isNavigatingOut;
    }

    public isActionInProgress(actionName?: string): boolean {
        if (actionName) {
            return this.actionsInProgress.indexOf(actionName) != -1;
        } else {
            return this.actionsInProgress.length >  0;
        }
    }

    public addActionInProgress(actionName: string): void {
        this.actionsInProgress.push(actionName);
        this.actionInProgressSubject.next(true);
    }

    public removeActionInProgress(actionName: string): void {
        let index: number = this.actionsInProgress.indexOf(actionName);
        this.actionsInProgress.splice(index, 1);
        this.actionInProgressSubject.next(this.actionsInProgress.length > 0);
    }

    public clearActionsInProgress(): void {
        this.actionsInProgress.length = 0;
        this.actionInProgressSubject.next(false);
    }
}
