import { CanDeactivateFn } from '@angular/router';
import { MemberDetail } from '../../features/members/member-detail/member-detail';

export const preventUnsavedChangesGuard: CanDeactivateFn<MemberDetail> = (component) => {
  if (component.editForm?.dirty) {
    return confirm('Are you sure you want to continue? All unsaved profile changes will be lost.');
  }

  return true;
};
