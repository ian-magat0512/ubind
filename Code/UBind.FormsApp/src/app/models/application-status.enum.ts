export enum ApplicationStatus {
    Nascent = 'nascent',
    Incomplete = 'incomplete',
    Review = 'review',
    Endorsement = 'endorsement', // only applicable for quotes
    Approved = 'approved',
    Notified = 'notified', // only applicable for claims
    Acknowledged = 'acknowledged', // only applicable for claims
    Assessment = 'assessment', // only applicable for claims
    Withdrawn = 'withdrawn', // only applicable for claims
    Complete = 'complete',
}
