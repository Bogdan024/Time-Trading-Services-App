import { Component, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AvailabilitySlotItem, MemberAvailabilitySlotEdit } from '../../types/member';
import { availabilityDays, availabilityModes, dayName, endHours, modeName, startHours } from '../member-profile-options';

export type AvailabilitySlotRemove = {
  slot: AvailabilitySlotItem;
  index: number;
};

@Component({
  selector: 'app-availability-editor',
  imports: [ReactiveFormsModule],
  templateUrl: './availability-editor.html',
  styleUrl: './availability-editor.css',
})
export class AvailabilityEditor {
  private fb = inject(FormBuilder);

  title = input('Availability');
  description = input('');
  emptyText = input('No availability added yet.');
  addButtonText = input('Add availability');
  closeButtonText = input('');
  layout = input<'compact' | 'wide'>('wide');
  slots = input<AvailabilitySlotItem[]>([]);
  addSlot = output<MemberAvailabilitySlotEdit>();
  removeSlot = output<AvailabilitySlotRemove>();
  closeEditor = output<void>();

  protected validationError = signal('');
  protected days = availabilityDays;
  protected startHours = startHours;
  protected endHours = endHours;
  protected availabilityModes = availabilityModes;
  protected form = this.fb.group({
    dayOfWeek: [1],
    startHour: [9],
    endHour: [10],
    mode: [3],
  });

  protected addAvailabilitySlot() {
    const slot = {
      dayOfWeek: Number(this.form.value.dayOfWeek),
      startHour: Number(this.form.value.startHour),
      endHour: Number(this.form.value.endHour),
      mode: Number(this.form.value.mode),
    };

    if (slot.endHour <= slot.startHour) {
      this.validationError.set('End hour must be after start hour');
      return;
    }

    if (this.slotExists(slot)) {
      this.validationError.set('This availability slot already exists');
      return;
    }

    this.validationError.set('');
    this.addSlot.emit(slot);
    this.form.reset({
      dayOfWeek: slot.dayOfWeek,
      startHour: 9,
      endHour: 10,
      mode: slot.mode,
    });
  }

  protected dayName(slot: AvailabilitySlotItem) {
    return dayName(slot.dayOfWeek);
  }

  protected modeName(slot: AvailabilitySlotItem) {
    return modeName(slot.mode);
  }

  private slotExists(slot: MemberAvailabilitySlotEdit) {
    return this.slots().some(
      (existingSlot) =>
        existingSlot.dayOfWeek === slot.dayOfWeek &&
        existingSlot.startHour === slot.startHour &&
        existingSlot.endHour === slot.endHour
    );
  }
}
