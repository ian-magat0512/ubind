import { ResumeApplicationService } from "./resume-application.service";
import { ResilientStorage } from '@app/storage/resilient-storage';
import { ApplicationService } from './application.service';
import { SessionDataManager } from '../storage/session-data-manager';

describe('ResumeApplicationService', () => {
    let applicationService: ApplicationService;
    let resilientStorage: ResilientStorage;
    let session: SessionDataManager;

    beforeEach(() => {
        applicationService = new ApplicationService();
        applicationService.setApplicationConfiguration(
            'https://localhost',
            'test-tenant',
            'test-tenant',
            'test-organisation',
            'test-organisation',
            false,
            'test-productId',
            'test-product',
            'development',
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
        resilientStorage = new ResilientStorage();
        session = new SessionDataManager();
    });

    it('should save and load the quote id', () => {
        // Arrange
        const applicationService: ApplicationService = new ApplicationService();
        const service: ResumeApplicationService = new ResumeApplicationService(
            resilientStorage,
            session,
            applicationService);
        const quoteId: string = "someQuoteId1";

        // Act
        service.saveQuoteIdForLater(quoteId, 30);

        // Assert
        expect(service.loadExistingQuoteId()).toBe('someQuoteId1');
    });

    it('should save and load the claim id', () => {
        // Arrange
        const service: ResumeApplicationService = new ResumeApplicationService(
            resilientStorage,
            session,
            applicationService);
        const quoteId: string = "someClaimId2";

        // Act 
        service.saveClaimIdForLater(quoteId, 30);

        // Assert
        expect(service.loadExistingClaimId()).toBe('someClaimId2');
    });

    it('should save and not load an expired quote id', () => {
        // Arrange
        let now: Date = new Date();
        now.setDate(now.getDate() - 5);
        const service: ResumeApplicationService = new ResumeApplicationService(
            resilientStorage,
            session,
            applicationService);
        const quoteId: string = "someQuoteId3";

        // Act 
        service.saveQuoteIdForLater(quoteId, 4, now);

        // Assert
        expect(service.loadExistingQuoteId()).toBeFalsy();
    });

    it('should delete a quote id', () => {
        // Arrange
        const service: ResumeApplicationService = new ResumeApplicationService(
            resilientStorage,
            session,
            applicationService);
        const quoteId: string = "someQuoteId4";
        service.saveQuoteIdForLater(quoteId, 30);

        // Act
        service.deleteQuoteId();

        // Assert
        expect(service.loadExistingQuoteId()).toBeFalsy();
    });
});
