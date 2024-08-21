import { ApplicationService } from '@app/services/application.service';
import { EventService } from '@app/services/event.service';
import { SectionWidget } from './section.widget';

describe('SectionWidget', () => {

    // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
    function getTestArticles(): Array<any> {
        return [
            { render: true, index: 0, sectionArticleElementStartingIndex: 0, sectionArticleElementEndingIndex: 2 },
            { render: true, index: 1, sectionArticleElementStartingIndex: 3, sectionArticleElementEndingIndex: 5 },
            { render: false, index: 2, sectionArticleElementStartingIndex: 6, sectionArticleElementEndingIndex: 6 },
            { render: true, index: 3, sectionArticleElementStartingIndex: 7, sectionArticleElementEndingIndex: 8 },
            { render: false, index: 4, sectionArticleElementStartingIndex: 9, sectionArticleElementEndingIndex: 9 },
            { render: false, index: 5, sectionArticleElementStartingIndex: 10, sectionArticleElementEndingIndex: 10 },
            { render: true, index: 6, sectionArticleElementStartingIndex: 11, sectionArticleElementEndingIndex: 11 },
        ];
    }

    // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
    function withArticleElementRenderStatuses(testArticles: Array<any>): Array<any> {
        testArticles[0].articleElementRenderStatuses = [
            { render: true, sectionArticleElementIndex: 0 },
            { render: true, sectionArticleElementIndex: 1 },
            { render: true, sectionArticleElementIndex: 2 },
        ];
        testArticles[1].articleElementRenderStatuses = [
            { render: true, sectionArticleElementIndex: 3 },
            { render: true, sectionArticleElementIndex: 4 },
            { render: false, sectionArticleElementIndex: 5 },
        ];
        testArticles[2].articleElementRenderStatuses = [
            { render: false, sectionArticleElementIndex: 6 },
        ];
        testArticles[3].articleElementRenderStatuses = [
            { render: false, sectionArticleElementIndex: 7 },
            { render: true, sectionArticleElementIndex: 8 },
        ];
        testArticles[4].articleElementRenderStatuses = [
            { render: true, sectionArticleElementIndex: 9 },
        ];
        testArticles[5].articleElementRenderStatuses = [
            { render: true, sectionArticleElementIndex: 10 },
        ];
        testArticles[6].articleElementRenderStatuses = [
            { render: true, sectionArticleElementIndex: 11 },
        ];
        return testArticles;
    }

    it('getFirstArticleIndex should return the first article index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = getTestArticles();

        // Act
        const firstArticleIndex1: number = sectionWidget.getFirstArticleIndex();
        sectionWidget.articles[0].render = false;
        const firstArticleIndex2: number = sectionWidget.getFirstArticleIndex();
        sectionWidget.articles[1].render = false;
        const firstArticleIndex3: number = sectionWidget.getFirstArticleIndex();

        // Assert
        expect(firstArticleIndex1).toBe(0);
        expect(firstArticleIndex2).toBe(1);
        expect(firstArticleIndex3).toBe(3);
    });

    it('getLastArticleIndex should return the last article index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = getTestArticles();

        // Act
        const firstArticleIndex1: number = sectionWidget.getLastArticleIndex();
        sectionWidget.articles[6].render = false;
        sectionWidget.articles[5].render = true;
        const firstArticleIndex2: number = sectionWidget.getLastArticleIndex();
        sectionWidget.articles[5].render = false;
        const firstArticleIndex3: number = sectionWidget.getLastArticleIndex();

        // Assert
        expect(firstArticleIndex1).toBe(6);
        expect(firstArticleIndex2).toBe(5);
        expect(firstArticleIndex3).toBe(3);
    });

    it('getNextArticleIndex should return the next article index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = getTestArticles();

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'test', articleIndex: 0 };
        const nextArticleIndex1: number = sectionWidget.getNextArticleIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleIndex: 1 };
        const nextArticleIndex2: number = sectionWidget.getNextArticleIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleIndex: 3 };
        const nextArticleIndex3: number = sectionWidget.getNextArticleIndex();

        // Assert
        expect(nextArticleIndex1).toBe(1);
        expect(nextArticleIndex2).toBe(3);
        expect(nextArticleIndex3).toBe(6);
    });

    it('getPreviousArticleIndex should return the previous article index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = getTestArticles();

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'test', articleIndex: 6 };
        const previousArticleIndex1: number = sectionWidget.getPreviousArticleIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleIndex: 3 };
        const previousArticleIndex2: number = sectionWidget.getPreviousArticleIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleIndex: 1 };
        const previousArticleIndex3: number = sectionWidget.getPreviousArticleIndex();

        // Assert
        expect(previousArticleIndex1).toBe(3);
        expect(previousArticleIndex2).toBe(1);
        expect(previousArticleIndex3).toBe(0);
    });

    it('getFirstArticleElementIndex should return the first article element index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = withArticleElementRenderStatuses(getTestArticles());

        // Act
        const firstArticleElementIndex1: number = sectionWidget.getFirstArticleElementIndex();
        sectionWidget.articles[0].render = false;
        const firstArticleElementIndex2: number = sectionWidget.getFirstArticleElementIndex();

        // Assert
        expect(firstArticleElementIndex1).toBe(0);
        expect(firstArticleElementIndex2).toBe(3);
    });

    it('getLastArticleElementIndex should return the last article element index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = withArticleElementRenderStatuses(getTestArticles());

        // Act
        sectionWidget.articles[6].render = true;
        const lastArticleElementIndex1: number = sectionWidget.getLastArticleElementIndex();
        sectionWidget.articles[6].render = false;
        sectionWidget.articles[5].render = true;
        const lastArticleElementIndex2: number = sectionWidget.getLastArticleElementIndex();
        sectionWidget.articles[5].render = false;
        sectionWidget.articles[4].render = false;
        const lastArticleElementIndex3: number = sectionWidget.getLastArticleElementIndex();

        // Assert
        expect(lastArticleElementIndex1).toBe(11);
        expect(lastArticleElementIndex2).toBe(10);
        expect(lastArticleElementIndex3).toBe(8);
    });

    it('getNextArticleElementIndex should return the next article element index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = withArticleElementRenderStatuses(getTestArticles());

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 0 };
        const nextArticleElementIndex1: number = sectionWidget.getNextArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 4 };
        const nextArticleElementIndex2: number = sectionWidget.getNextArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 2 };
        const nextArticleElementIndex3: number = sectionWidget.getNextArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 3 };
        const nextArticleElementIndex4: number = sectionWidget.getNextArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 11 };
        const nextArticleElementIndex5: number = sectionWidget.getNextArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 12 };
        const nextArticleElementIndex6: number = sectionWidget.getNextArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: -1 };
        const nextArticleElementIndex7: number = sectionWidget.getNextArticleElementIndex();

        // Assert
        expect(nextArticleElementIndex1).toBe(1);
        expect(nextArticleElementIndex2).toBe(8);
        expect(nextArticleElementIndex3).toBe(3);
        expect(nextArticleElementIndex4).toBe(4);
        expect(nextArticleElementIndex5).toBe(-1);
        expect(nextArticleElementIndex6).toBe(-1);
        expect(nextArticleElementIndex7).toBe(-1);
    });

    it('getPreviousArticleElementIndex should return the previous article element index that\'s renderable', () => {
        // Arrange
        let applicationService: ApplicationService = new ApplicationService();
        let sectionWidget: SectionWidget = new SectionWidget(
            null, null, null, null, null, applicationService, new EventService());
        sectionWidget.articles = withArticleElementRenderStatuses(getTestArticles());

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 11 };
        const previousArticleElementIndex1: number = sectionWidget.getPreviousArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 8 };
        const previousArticleElementIndex2: number = sectionWidget.getPreviousArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 4 };
        const previousArticleElementIndex3: number = sectionWidget.getPreviousArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 1 };
        const previousArticleElementIndex4: number = sectionWidget.getPreviousArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: 0 };
        const previousArticleElementIndex5: number = sectionWidget.getPreviousArticleElementIndex();
        applicationService.currentWorkflowDestination = { stepName: 'test', articleElementIndex: -1 };
        const previousArticleElementIndex6: number = sectionWidget.getPreviousArticleElementIndex();

        // Assert
        expect(previousArticleElementIndex1).toBe(10);
        expect(previousArticleElementIndex2).toBe(4);
        expect(previousArticleElementIndex3).toBe(3);
        expect(previousArticleElementIndex4).toBe(0);
        expect(previousArticleElementIndex5).toBe(-1);
        expect(previousArticleElementIndex6).toBe(-1);
    });
});
