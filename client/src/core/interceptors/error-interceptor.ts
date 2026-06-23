import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { ToastService } from '../services/toast-service';
import { AccountService } from '../services/account-service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);
  const accountService = inject(AccountService);

  return next(req).pipe(
    catchError((error) => {
      if (error) {
        switch (error.status) {
          case 400:
            if (error.error.errors) {
              const modelStateErrors: string[] = [];
              for (const key in error.error.errors) {
                if (error.error.errors[key]) {
                  modelStateErrors.push(error.error.errors[key]);
                }
              }

              return throwError(() => modelStateErrors.flat());
            } else {
              toast.error(error.error);
            }
            break;
          case 401:
            if (isRefreshRequest(req.url)) {
              accountService.clearCurrentUser();
              return throwError(() => error);
            }

            if (shouldAttemptRefresh(req.url, accountService)) {
              return accountService.refreshToken().pipe(
                switchMap(() => next(req)),
                catchError((refreshError) => {
                  accountService.clearCurrentUser();
                  toast.error('Session expired. Please sign in again.');
                  return throwError(() => refreshError);
                }),
              );
            }

            toast.error('Unauthorized');
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500: {
            const navigationExtras: NavigationExtras = {
              state: { error: error.error },
            };
            router.navigateByUrl('/server-error', navigationExtras);
            break;
          }
          default:
            toast.error('Something went wrong');
            break;
        }
      }

      return throwError(() => error);
    }),
  );
};

function shouldAttemptRefresh(url: string, accountService: AccountService) {
  return !!accountService.currentUser() && !isAuthRequest(url);
}

function isAuthRequest(url: string) {
  return url.includes('account/login')
    || url.includes('account/register')
    || url.includes('account/refresh-token')
    || url.includes('account/revoke-token');
}

function isRefreshRequest(url: string) {
  return url.includes('account/refresh-token');
}

