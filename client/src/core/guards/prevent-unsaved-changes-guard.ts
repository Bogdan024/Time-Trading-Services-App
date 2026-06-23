import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { MemberDetail } from '../../features/members/member-detail/member-detail';
import { ConfirmDialogService } from '../services/confirm-dialog-service';

export const preventUnsavedChangesGuard: CanDeactivateFn<MemberDetail> = (component) => {
  if (component.editForm?.dirty) {
    return inject(ConfirmDialogService).confirm(
      'Leave this page?',
      'All unsaved profile changes will be lost if you continue.'
    );
  }

  return true;
};
