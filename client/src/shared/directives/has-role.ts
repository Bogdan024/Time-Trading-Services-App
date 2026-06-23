import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';
import { AccountService } from '../../core/services/account-service';

@Directive({
  selector: '[appHasRole]',
})
export class HasRole {
  private accountService = inject(AccountService);
  private viewContainerRef = inject(ViewContainerRef);
  private templateRef = inject(TemplateRef<unknown>);
  appHasRole = input.required<string[]>();

  constructor() {
    effect(() => {
      this.viewContainerRef.clear();

      if (this.accountService.hasRole(this.appHasRole())) {
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      }
    });
  }
}
