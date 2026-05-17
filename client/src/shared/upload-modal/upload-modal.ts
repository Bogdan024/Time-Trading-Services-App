import { Component, HostListener, input, output } from '@angular/core';
import { ImageUpload } from '../image-upload/image-upload';

@Component({
  selector: 'app-upload-modal',
  imports: [ImageUpload],
  templateUrl: './upload-modal.html',
  styleUrl: './upload-modal.css',
})
export class UploadModal {
  loading = input(false);
  eyebrow = input('Upload');
  title = input('Upload image');
  description = input('');
  closeModal = output<void>();
  uploadFile = output<File>();

  @HostListener('window:keydown.escape')
  closeOnEscape() {
    this.close();
  }

  protected close() {
    if (this.loading()) return;

    this.closeModal.emit();
  }

  protected onUploadFile(file: File) {
    this.uploadFile.emit(file);
  }
}
