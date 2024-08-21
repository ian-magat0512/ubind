import { AlertWidget } from './alert.widget';
import { AlertService } from '@app/services/alert.service';
import { Alert } from '@app/models/alert';
import { MessageService } from '@app/services/message.service';
import { EventService } from '@app/services/event.service';

describe('AlertWidget', () => {

    let widget: AlertWidget;
    let notificationService: AlertService;
    let eventService: EventService;
    let messageService: MessageService;

    beforeEach(() => {
        eventService = new EventService();
        messageService = new MessageService(eventService, null, null, null);
        notificationService = new AlertService(messageService, eventService);
        widget = new AlertWidget(notificationService, eventService);
    });

    it('Should be created.', () => {
        expect(widget).toBeTruthy();
    });

    it('should show up when title and message is not null or empty.', () => {
        const errorDetail: Alert = new Alert('Test title', 'Test message');
        widget.update(errorDetail);
        expect(widget.visible).toBeTruthy();
    });

    it('should NOT show up when title or message is empty.', () => {
        AlertWidget.suppressErrors = true;
        const errorDetail: Alert = new Alert('', '');
        widget.update(errorDetail);
        expect(widget.visible).toBeFalsy();
    });

    it('should NOT show up when title or message is null', () => {
        AlertWidget.suppressErrors = true;
        const errorDetail: Alert = new Alert(null, null);
        widget.update(errorDetail);
        expect(widget.visible).toBeFalsy();
    });
});
