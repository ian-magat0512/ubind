import { UserResourceModel } from '@app/resource-models/user/user.resource-model';

/**
 * Export user Summarry View Model Class.
 * TODO: Write a better class header: view model of user summary.
 */
export class UserSummaryViewModel {
    public constructor(user: UserResourceModel) {
        this.id = user["id"];
        this.fullName = user.fullName;
    }

  public id: string;
  public fullName: string;
}
