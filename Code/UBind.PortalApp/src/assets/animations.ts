import { trigger, state, style, transition, animate } from '@angular/animations';

export const contentAnimation: any = trigger('contentAnimation', [
    state('*', style({ opacity: 1 })),
    state('void', style({ opacity: 0 })),
    transition('void => *', [
        animate('200ms ease-in-out'),
    ]),
    transition('* => void', [
        animate('200ms 50ms ease-in-out'),
    ]),
]);

export const searchAnimation: any = trigger('searchAnimation', [
    state('*', style({ opacity: 1 })),
    state('void', style({ opacity: 0 })),
    transition('void => *', [
        animate('200ms ease-in-out'),
    ]),
]);

export const loaderAnimation: any = trigger('loaderAnimation', [
    state('*', style({ opacity: 1 })),
    state('void', style({ opacity: 0 })),
    transition('void => *', [
        animate('500ms 500ms ease-in-out'),
    ]),
    transition('* => void', [
        animate('300ms ease-in-out'),
    ]),
]);

export const slideupAnimation: any = trigger('slideupAnimation', [
    state('*', style({ opacity: 1 })),
    state('void', style({ opacity: 0 })),
    transition('void => *', [
        animate('500ms 500ms ease-in-out'),
    ]),
    transition('* => void', [
        animate('300ms ease-in-out'),
    ]),
]);
