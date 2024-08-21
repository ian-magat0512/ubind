export enum CalculationState {
    Incomplete = 'incomplete',
    PremiumEstimate = 'premiumEstimate',
    PremiumComplete = 'premiumComplete',
    BindingQuote = 'bindingQuote',
    PaymentDetailsComplete = 'paymentDetailsComplete',
    LegalTermsAccepted = 'legalTermsAccepted',
}

export enum TriggerState {
    SoftReferral = 'softReferral',
    HardReferral = 'hardReferral',
    Endorsement = 'endorsement',
    Review = 'review',
    Error = 'error',
    Decline = 'decline',
    Assessment = 'assessment',
    Notification = 'notification'
}
