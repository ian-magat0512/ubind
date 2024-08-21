import { WorkflowDestination } from "./workflow-destination";

/**
 * A navigation from one workflow destination to another.
 */
export class WorkflowNavigation {
    public from: WorkflowDestination;
    public to: WorkflowDestination;

    public constructor(from: WorkflowDestination, to: WorkflowDestination) {
        this.from = from;
        this.to = to;
    }

    public isDifferentStep(): boolean {
        return this.from?.stepName != this.to.stepName;
    }

    public isDifferentArticleElement(): boolean {
        return this.from.articleElementIndex != this.to.articleElementIndex;
    }
}
