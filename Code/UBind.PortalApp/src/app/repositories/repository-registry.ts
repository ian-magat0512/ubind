import { Injectable } from "@angular/core";
import { IncrementalDataRepository } from "./incremental-data.repository";

/* eslint-disable @typescript-eslint/ban-types */

/**
 * Export Repository Registry class.
 * TODO: Write a better class header: registry repository function.
 */
@Injectable({ providedIn: 'root' })
export class RepositoryRegistry {

    private repositories: Map<string, IncrementalDataRepository> = new Map<string, IncrementalDataRepository>();
    private referenceCounts: Map<string, number> = new Map<string, number>();

    /**
     * Gets a repository if it exists, if not, creates one using the factory method, and inserts it into the repository.
     * @param repositoryKey
     * @param factoryMethod
     */
    public getOrCreate(repositoryKey: string, factoryMethod: Function): IncrementalDataRepository {
        let repository: IncrementalDataRepository = this.repositories.get(repositoryKey);
        if (!repository) {
            this.referenceCounts.set(repositoryKey, 1);
            repository = factoryMethod();
            this.repositories.set(repositoryKey, repository);
        } else {
            let referenceCount: number = this.referenceCounts.get(repositoryKey);
            this.referenceCounts.set(repositoryKey, ++referenceCount);
        }
        return repository;
    }

    /**
     * Removes a repository from the registry
     * @param repositoryKey
     */
    public remove(repositoryKey: string): void {
        let referenceCount: number = this.referenceCounts.get(repositoryKey);
        this.referenceCounts.set(repositoryKey, --referenceCount);
        if (referenceCount == 0) {
            this.repositories.delete(repositoryKey);
        }
    }
}
