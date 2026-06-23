import { Component, ElementRef, ViewChild, inject } from '@angular/core';
import { ConfirmDialogService } from '../../core/services/confirm-dialog-service';

@Component({
  selector: 'app-confirm-dialog',
  imports: [],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css',
})
export class ConfirmDialog {
  @ViewChild('dialogRef') private dialogRef?: ElementRef<HTMLDialogElement>;

  protected message = 'Are you sure?';
  protected detail = 'This action cannot be undone.';
  private resolver?: (confirmed: boolean) => void;

  constructor() {
    inject(ConfirmDialogService).register(this);
  }

  open(message: string, detail = 'This action cannot be undone.') {
    this.message = message;
    this.detail = detail;
    this.dialogRef?.nativeElement.showModal();

    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
    });
  }

  protected confirm() {
    this.close(true);
  }

  protected cancel() {
    this.close(false);
  }

  protected closeFromBackdrop(event: MouseEvent) {
    if (event.target === this.dialogRef?.nativeElement) {
      this.close(false);
    }
  }

  private close(confirmed: boolean) {
    this.dialogRef?.nativeElement.close();
    this.resolver?.(confirmed);
    this.resolver = undefined;
  }
}
