import { of } from 'rxjs';
import { SystemAlertApiService } from './system-alert-api.service';

describe('SystemAlertApiService', () => {
    let saService: SystemAlertApiService;

    beforeEach(() => {
        const appConfigServiceStub: any = {};
        const configStub: any = {
            portal: {
                api: {
                    baseUrl: "api/v1/system-alert",
                },
            },
        };
        appConfigServiceStub.appConfigSubject = of({ ...configStub });
        saService = new SystemAlertApiService(null, appConfigServiceStub);
    });

    afterEach(() => {
        saService = null;
    });

    it('should be created', () => {
        expect(saService).toBeTruthy();
    });
});
