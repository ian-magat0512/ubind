/**
 * Export stripe token response class.
 * TODO: Write a better class header: stripe token functions.
 */
export class StripeTokenResponse {

    public id: string;

    public constructor(object: any) {
        this.id = object.id;
        if (!this.id) {
            throw new Error("Stripe token response did not include id.");
        }
    }
}
