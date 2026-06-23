import { Injectable } from '@angular/core';
import { ConfirmDialog } from '../../shared/confirm-dialog/confirm-dialog';

@Injectable({
  providedIn: 'root',
})
export class ConfirmDialogService {
  private dialog?: ConfirmDialog;

  register(dialog: ConfirmDialog) {
    this.dialog = dialog;
  }

  confirm(message = 'Are you sure?', detail = 'This action cannot be undone.') {
    if (!this.dialog) {
      return Promise.resolve(window.confirm(message));
    }

    return this.dialog.open(message, detail);
  }
}
