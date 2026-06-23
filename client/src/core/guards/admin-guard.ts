import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../services/account-service';
import { ToastService } from '../services/toast-service';

export const adminGuard: CanActivateFn = () => {
  const accountService = inject(AccountService);
  const router = inject(Router);
  const toast = inject(ToastService);

  if (accountService.hasRole(['Admin'])) {
    return true;
  }

  toast.error('Admin access required');
  return router.createUrlTree(['/tasks']);
};
