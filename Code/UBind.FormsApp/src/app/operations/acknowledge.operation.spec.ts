import { AcknowledgeOperation } from "./acknowledge.operation";
import { ApplicationService } from "../services/application.service";

/**
 * Mock application service class
 */
class MockApplicationService extends ApplicationService {
    private formTypeMock: string;

    public get formType(): string {
        return this.formTypeMock;
    }

    public set formType(form: string) {
        this.formTypeMock = form;
    }

    public setFormType(formType: string): void {
        this.formType = formType;
    }
}
describe('AcknowledgeOperation', () => {
    let acknowledgeOperation: AcknowledgeOperation;

    afterEach(() => {
        acknowledgeOperation = null;
    });

    it('should raise an error when operation is not allowed for given formtype', () => {
        // Arrange
        const mockApplicationService: MockApplicationService = new MockApplicationService();
        mockApplicationService.setFormType('quote');
        acknowledgeOperation = new AcknowledgeOperation(null, null, mockApplicationService, null, null, null, null);

        // Act
        expect(() => {
            acknowledgeOperation.execute(null, null, 12345678).subscribe(() => null);
        })
            .toThrow(new Error('Operation not allowed for this form type quote'));
    });

});
