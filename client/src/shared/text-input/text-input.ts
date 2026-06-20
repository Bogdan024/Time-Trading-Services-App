import { Component, input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  imports: [ReactiveFormsModule],
  templateUrl: './text-input.html',
  styleUrl: './text-input.css',
})
export class TextInput implements ControlValueAccessor {
  label = input('');
  type = input('text');
  maxLength = input<number | null>(null);
  placeholder = input('');

  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
  }

  writeValue(_value: unknown): void {}

  registerOnChange(_fn: unknown): void {}

  registerOnTouched(_fn: unknown): void {}

  get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
