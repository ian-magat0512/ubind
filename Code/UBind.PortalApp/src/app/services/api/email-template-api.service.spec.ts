import { AppConfig } from '@app/models/app-config';
import { BehaviorSubject } from 'rxjs';
import { EmailTemplateApiService } from '@app/services/api/email-template-api.service';

describe('EmailTemplateApiService', () => {
    let service: EmailTemplateApiService;
    let mockAppConfigServiceStub: any;

    beforeEach(() => {
        mockAppConfigServiceStub = {
            appConfigSubject: new BehaviorSubject<AppConfig>(<AppConfig>{
                portal: {
                    api: {
                        baseUrl: 'https://mock.ignore/',
                    },
                },
            }),
        };

        service = new EmailTemplateApiService(null, mockAppConfigServiceStub);
    });

    afterEach(() => {
        service = null;
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });
});
