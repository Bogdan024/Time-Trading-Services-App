import { Component, input, output } from '@angular/core';
import { ServiceCategory } from '../../../types/member';
import { TaskLocationMode, TaskParams } from '../../../types/task';

@Component({
  selector: 'app-task-filters',
  imports: [],
  templateUrl: './task-filters.html',
  styleUrl: './task-filters.css',
})
export class TaskFilters {
  categories = input<ServiceCategory[]>([]);
  taskParams = input.required<TaskParams>();
  applyFilters = output<TaskParams>();
  resetFilters = output<void>();

  protected readonly creditOptions = Array.from({ length: 10 }, (_, index) => index + 1);
  protected readonly locationModes: { value: TaskLocationMode; label: string }[] = [
    { value: 1, label: 'In person' },
    { value: 2, label: 'Remote' },
    { value: 3, label: 'Either' },
  ];

  protected setNumberFilter(field: 'serviceCategoryId' | 'minCredits' | 'maxCredits' | 'minPosterRating', target: EventTarget | null) {
    const value = this.getTargetValue(target);
    this.taskParams()[field] = value ? Number(value) : undefined;
  }

  protected setLocationMode(target: EventTarget | null) {
    const value = this.getTargetValue(target);
    this.taskParams().locationMode = value ? Number(value) as TaskLocationMode : undefined;
  }

  protected setDueSoon(target: EventTarget | null) {
    this.taskParams().dueSoon = target instanceof HTMLInputElement ? target.checked : false;
  }

  protected setOrderBy(target: EventTarget | null) {
    const value = this.getTargetValue(target);
    this.taskParams().orderBy = value || 'newest';
  }
  protected apply() {
    const params = this.taskParams();

    this.applyFilters.emit({
      ...params,
      city: undefined,
      countryCode: undefined,
    });
  }

  private getTargetValue(target: EventTarget | null) {
    return target instanceof HTMLInputElement || target instanceof HTMLSelectElement ? target.value : '';
  }
}

