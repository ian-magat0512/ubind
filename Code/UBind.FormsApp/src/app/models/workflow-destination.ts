/**
 * A workflow destination is a place in the workflow that can be navigated to
 */
export interface WorkflowDestination {
    stepName: string;
    articleIndex?: number;
    articleElementIndex?: number;
    repeatingInstanceIndex?: number;

    /**
     * If you have more than one repeating question displayed at a step, you can 
     * target a specific one by passing in the fieldPath of it
     */
    repeatingFieldPath?: string;
}
