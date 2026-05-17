import { Component, input, output, signal } from '@angular/core';

@Component({
  selector: 'app-image-upload',
  imports: [],
  templateUrl: './image-upload.html',
  styleUrl: './image-upload.css',
})
export class ImageUpload {
  loading = input(false);
  uploadFile = output<File>();
  protected imageSrc = signal<string | ArrayBuffer | null>(null);
  protected isDragging = signal(false);
  private fileToUpload?: File;

  protected onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    this.previewImage(file);
  }

  protected onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  protected onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  protected onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const file = event.dataTransfer?.files[0];

    if (!file) return;

    this.previewImage(file);
  }

  protected onCancel() {
    this.fileToUpload = undefined;
    this.imageSrc.set(null);
  }

  protected onUploadFile() {
    if (!this.fileToUpload) return;

    this.uploadFile.emit(this.fileToUpload);
  }

  private previewImage(file: File) {
    if (!file.type.startsWith('image/')) return;

    this.fileToUpload = file;
    const reader = new FileReader();
    reader.onload = (event) => this.imageSrc.set(event.target?.result ?? null);
    reader.readAsDataURL(file);
  }
}
