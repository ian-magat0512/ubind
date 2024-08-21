import { Injectable } from "@angular/core";
import { EventService } from "./event.service";

interface QuestionSetCompletionStats {
    numGroups: number;
    numGroupsCompleted: number;
}

/**
 * Tracks the completion status of question sets grouped by section
 */
@Injectable()
export class RevealGroupTrackingService {

    private stepsCompletionMap: Map<string, Map<string, QuestionSetCompletionStats>>
        = new Map<string, Map<string, QuestionSetCompletionStats>>();

    private stepsUsingProgressiveReveal: Set<string> = new Set<string>();

    public constructor(private eventService: EventService) {
    }

    public registerStepUsingProgressiveReveal(stepName: string): void {
        this.stepsUsingProgressiveReveal.add(stepName);
    }

    public doesStepUseProgressiveReveal(stepName: string): boolean {
        return this.stepsUsingProgressiveReveal.has(stepName);
    }

    public updateCompletion(
        stepName: string,
        questionSetPath: string,
        numGroups: number,
        numGroupsCompleted: number,
    ): void {
        let questionSetsCompletion: Map<string, QuestionSetCompletionStats> = this.stepsCompletionMap.get(stepName);
        if (!questionSetsCompletion) {
            questionSetsCompletion = new Map<string, QuestionSetCompletionStats>();
            this.stepsCompletionMap.set(stepName, questionSetsCompletion);
        }

        questionSetsCompletion.set(questionSetPath, { numGroups, numGroupsCompleted });
        this.publishCompletion(questionSetsCompletion);
    }

    private publishCompletion(questionSetsCompletion: Map<string, QuestionSetCompletionStats>): void {
        let totalGroups: number = 0;
        let totalGroupsCompleted: number = 0;
        questionSetsCompletion.forEach((stats: QuestionSetCompletionStats) => {
            totalGroups += stats.numGroups;
            totalGroupsCompleted += stats.numGroupsCompleted;
        });
        let percentage: number = (totalGroupsCompleted / totalGroups) * 100.0;
        this.eventService.stepCompletionSubject.next(percentage);
    }
}
