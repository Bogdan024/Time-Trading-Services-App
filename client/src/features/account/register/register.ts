import { Component, inject, OnInit, output, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { AccountService } from '../../../core/services/account-service';
import { MemberService } from '../../../core/services/member-service';
import { ServiceCategoryService } from '../../../core/services/service-category-service';
import { AvailabilityEditor, AvailabilitySlotRemove } from '../../../shared/availability-editor/availability-editor';
import { LocationPicker } from '../../../shared/location-picker/location-picker';
import { ServicePreferenceEditor } from '../../../shared/service-preference-editor/service-preference-editor';
import { TextInput } from '../../../shared/text-input/text-input';
import {
  MemberAvailabilitySlotEdit,
  MemberServiceCategoryEdit,
  ServiceCategory,
  ServicePreferenceItem,
} from '../../../types/member';
import { TaskLocation } from '../../../types/task';
import { RegisterCreds } from '../../../types/user';

@Component({
  selector: 'app-register',
  imports: [AvailabilityEditor, LocationPicker, ReactiveFormsModule, ServicePreferenceEditor, TextInput],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register implements OnInit {
  private accountService = inject(AccountService);
  private memberService = inject(MemberService);
  private serviceCategoryService = inject(ServiceCategoryService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  cancelRegister = output<boolean>();
  protected currentStep = signal(1);
  protected loading = signal(false);
  protected validationErrors = signal<string[]>([]);
  protected serviceCategories = signal<ServiceCategory[]>([]);
  protected selectedSkills = signal<MemberServiceCategoryEdit[]>([]);
  protected selectedNeeds = signal<MemberServiceCategoryEdit[]>([]);
  protected selectedAvailabilitySlots = signal<MemberAvailabilitySlotEdit[]>([]);
  protected selectedProfileLocation = signal<TaskLocation | null>(null);
  protected readonly steps = ['Account', 'Profile', 'Skills', 'Needs', 'Availability', 'Review'];

  protected credentialsForm: FormGroup;
  protected profileForm: FormGroup;

  constructor() {
    this.credentialsForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', [Validators.required, Validators.maxLength(80)]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(64), this.passwordPolicyValidator()]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]],
    });

    this.profileForm = this.fb.group({
      city: ['', [Validators.required, Validators.maxLength(80)]],
      countryCode: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(2), Validators.pattern(/^[a-zA-Z]{2}$/)]],
      about: ['', Validators.maxLength(1000)],
      isProfilePublic: [true],
    });

    this.credentialsForm.controls['password'].valueChanges.subscribe(() => {
      this.credentialsForm.controls['confirmPassword'].updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    if (this.accountService.currentUser()) {
      this.router.navigateByUrl('/tasks');
      return;
    }

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
    if (this.currentStep() === 1) return this.credentialsForm.valid;
    if (this.currentStep() === 2) return this.profileForm.valid;

    return true;
  }

  protected onProfileLocationSelected(location: TaskLocation) {
    this.selectedProfileLocation.set(location);
    this.profileForm.patchValue({
      city: location.city,
      countryCode: location.countryCode,
    });
  }

  protected onProfileLocationCleared() {
    this.selectedProfileLocation.set(null);
    this.profileForm.patchValue({
      city: '',
      countryCode: '',
    });
  }

  protected addSkill(skill: MemberServiceCategoryEdit) {
    this.selectedSkills.update((skills) => [...skills, skill]);
  }

  protected removeSkill(skillToRemove: ServicePreferenceItem) {
    this.selectedSkills.update((skills) => skills.filter((skill) => skill.serviceCategoryId !== skillToRemove.serviceCategoryId));
  }

  protected addNeed(need: MemberServiceCategoryEdit) {
    this.selectedNeeds.update((needs) => [...needs, need]);
  }

  protected removeNeed(needToRemove: ServicePreferenceItem) {
    this.selectedNeeds.update((needs) => needs.filter((need) => need.serviceCategoryId !== needToRemove.serviceCategoryId));
  }

  protected addAvailabilitySlot(slot: MemberAvailabilitySlotEdit) {
    this.selectedAvailabilitySlots.update((slots) => [...slots, slot]);
  }

  protected removeAvailabilitySlot(slotToRemove: AvailabilitySlotRemove) {
    this.selectedAvailabilitySlots.update((slots) => slots.filter((_, index) => index !== slotToRemove.index));
  }

  protected register() {
    if (this.accountService.currentUser()) {
      this.router.navigateByUrl('/tasks');
      return;
    }

    if (this.credentialsForm.invalid || this.profileForm.invalid) {
      this.credentialsForm.markAllAsTouched();
      this.profileForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.validationErrors.set([]);

    const credentials = this.credentialsForm.value;
    const profile = this.profileForm.value;
    const registerCreds: RegisterCreds = {
      email: credentials.email.trim(),
      displayName: credentials.displayName.trim(),
      password: credentials.password,
      city: profile.city.trim(),
      countryCode: profile.countryCode.trim().toUpperCase(),
      about: profile.about?.trim() || undefined,
      isProfilePublic: profile.isProfilePublic,
    };

    this.accountService
      .register(registerCreds)
      .pipe(
        switchMap(() =>
          this.memberService.saveOnboardingProfileSetup({
            skills: this.selectedSkills(),
            needs: this.selectedNeeds(),
            availabilitySlots: this.selectedAvailabilitySlots(),
          }),
        ),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: () => this.router.navigateByUrl('/tasks'),
        error: (error) => this.validationErrors.set(this.normalizeErrors(error)),
      });
  }

  protected cancel() {
    this.cancelRegister.emit(false);
  }

  private passwordPolicyValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = String(control.value ?? '');
      const errors: ValidationErrors = {};

      if (!/\d/.test(value)) errors['passwordRequiresDigit'] = true;
      if (!/[a-z]/.test(value)) errors['passwordRequiresLowercase'] = true;
      if (!/[A-Z]/.test(value)) errors['passwordRequiresUppercase'] = true;

      return Object.keys(errors).length ? errors : null;
    };
  }

  private matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent;
      if (!parent) return null;

      return control.value === parent.get(matchTo)?.value ? null : { passwordMismatch: true };
    };
  }

  private markCurrentStepTouched() {
    if (this.currentStep() === 1) this.credentialsForm.markAllAsTouched();
    if (this.currentStep() === 2) {
      this.profileForm.markAllAsTouched();
    }
  }

  private normalizeErrors(error: unknown) {
    if (Array.isArray(error)) return error.map((item) => String(item));
    if (typeof error === 'string') return [error];

    return ['Something went wrong while creating your account'];
  }
}





