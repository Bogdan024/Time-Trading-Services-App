import { Component, inject, OnInit, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ServiceCategoryService } from '../../../core/services/service-category-service';
import { TaskService } from '../../../core/services/task-service';
import { ToastService } from '../../../core/services/toast-service';
import { TextInput } from '../../../shared/text-input/text-input';
import { ServiceCategory } from '../../../types/member';
import { CreateTimeTask, TaskLocationMode } from '../../../types/task';

@Component({
  selector: 'app-task-create',
  imports: [ReactiveFormsModule, RouterLink, TextInput],
  templateUrl: './task-create.html',
  styleUrl: './task-create.css',
})
export class TaskCreate implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private serviceCategoryService = inject(ServiceCategoryService);
  private toast = inject(ToastService);

  protected currentStep = signal(1);
  protected serviceCategories = signal<ServiceCategory[]>([]);
  protected validationErrors = signal<string[]>([]);
  protected loading = signal(false);
  protected readonly steps = ['Task details', 'Exchange and location', 'Due window', 'Review'];
  protected creditOptions = Array.from({ length: 10 }, (_, index) => index + 1);
  protected timeOptions = Array.from({ length: 13 }, (_, index) => `${(index + 8).toString().padStart(2, '0')}:00`);
  protected locationModes: { value: TaskLocationMode; label: string }[] = [
    { value: 1, label: 'In person' },
    { value: 2, label: 'Remote' },
    { value: 3, label: 'Either' },
  ];

  protected form = this.fb.group(
    {
      title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(20), Validators.maxLength(2000)]],
      serviceCategoryId: [0, [Validators.required, Validators.min(1)]],
      estimatedHours: [1, [Validators.required, Validators.min(1), Validators.max(10)]],
      locationMode: [3, Validators.required],
      city: ['', [Validators.required, Validators.maxLength(80)]],
      countryCode: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(2), Validators.pattern(/^[a-zA-Z]{2}$/)]],
      dueDate: [''],
      dueTime: [''],
    },
    { validators: this.futureDueDateValidator() },
  );

  ngOnInit(): void {
    this.serviceCategoryService.getServiceCategories().subscribe({
      next: (categories) => this.serviceCategories.set(categories),
    });
  }

  protected nextStep() {
    if (!this.canContinue()) {
      this.markCurrentStepTouched();
      return;
    }

    this.currentStep.update((step) => Math.min(step + 1, this.steps.length));
  }

  protected previousStep() {
    this.currentStep.update((step) => Math.max(step - 1, 1));
  }

  protected canContinue() {
    if (this.currentStep() === 1) {
      return this.form.controls.title.valid && this.form.controls.description.valid && this.form.controls.serviceCategoryId.valid;
    }

    if (this.currentStep() === 2) {
      return (
        this.form.controls.estimatedHours.valid &&
        this.form.controls.locationMode.valid &&
        this.form.controls.city.valid &&
        this.form.controls.countryCode.valid
      );
    }

    if (this.currentStep() === 3) {
      return !this.form.hasError('incompleteDueDate') && !this.form.hasError('dueDateInPast');
    }

    return this.form.valid;
  }

  protected selectedCategoryName() {
    return this.serviceCategories().find((category) => category.id === Number(this.form.value.serviceCategoryId))?.name ?? 'Category';
  }

  protected selectedLocationModeName() {
    return this.locationModes.find((mode) => mode.value === Number(this.form.value.locationMode))?.label ?? 'Either';
  }

  protected createTask() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.validationErrors.set([]);

    const value = this.form.value;
    const task: CreateTimeTask = {
      title: value.title?.trim() ?? '',
      description: value.description?.trim() ?? '',
      serviceCategoryId: Number(value.serviceCategoryId),
      estimatedHours: Number(value.estimatedHours),
      locationMode: Number(value.locationMode) as TaskLocationMode,
      city: value.city?.trim(),
      countryCode: value.countryCode?.trim().toUpperCase(),
      dueAtUtc: this.getDueAtUtc(),
    };

    this.taskService.createTask(task).subscribe({
      next: () => {
        this.toast.success('Task posted');
        this.router.navigateByUrl('/my-tasks/posted');
      },
      error: (error) => {
        this.loading.set(false);
        this.validationErrors.set(this.normalizeErrors(error));
      },
    });
  }

  private markCurrentStepTouched() {
    if (this.currentStep() === 1) {
      this.form.controls.title.markAsTouched();
      this.form.controls.description.markAsTouched();
      this.form.controls.serviceCategoryId.markAsTouched();
    }

    if (this.currentStep() === 2) {
      this.form.controls.estimatedHours.markAsTouched();
      this.form.controls.locationMode.markAsTouched();
      this.form.controls.city.markAsTouched();
      this.form.controls.countryCode.markAsTouched();
    }

    if (this.currentStep() === 3) {
      this.form.controls.dueDate.markAsTouched();
      this.form.controls.dueTime.markAsTouched();
    }
  }

  private getDueAtUtc() {
    const dueDate = this.form.value.dueDate;
    const dueTime = this.form.value.dueTime;

    if (!dueDate || !dueTime) return undefined;

    return new Date(`${dueDate}T${dueTime}`).toISOString();
  }

  private futureDueDateValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const dueDate = control.get('dueDate')?.value;
      const dueTime = control.get('dueTime')?.value;

      if (!dueDate && !dueTime) return null;
      if (!dueDate || !dueTime) return { incompleteDueDate: true };

      return new Date(`${dueDate}T${dueTime}`) > new Date() ? null : { dueDateInPast: true };
    };
  }

  private normalizeErrors(error: unknown) {
    if (Array.isArray(error)) return error.map((item) => String(item));
    if (typeof error === 'string') return [error];

    return ['Something went wrong while creating the task'];
  }
}
