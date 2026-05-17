import { DatePipe } from '@angular/common';
import { Component, HostListener, inject, OnInit, signal, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { MemberService } from '../../../core/services/member-service';
import { ServiceCategoryService } from '../../../core/services/service-category-service';
import { TaskService } from '../../../core/services/task-service';
import { ToastService } from '../../../core/services/toast-service';
import {
  EditableMember,
  Member,
  MemberAvailabilitySlot,
  MemberAvailabilitySlotEdit,
  MemberServiceCategoryEdit,
  ServiceCategory,
} from '../../../types/member';
import { TimeTransaction } from '../../../types/task';
import { UploadModal } from '../../../shared/upload-modal/upload-modal';

@Component({
  selector: 'app-member-detail',
  imports: [DatePipe, FormsModule, RouterLink, UploadModal],
  templateUrl: './member-detail.html',
  styleUrl: './member-detail.css',
})
export class MemberDetail implements OnInit {
  @ViewChild('editForm') editForm?: NgForm;

  @HostListener('window:beforeunload', ['$event'])
  notify($event: BeforeUnloadEvent) {
    if (this.editForm?.dirty) {
      $event.preventDefault();
    }
  }

  private route = inject(ActivatedRoute);
  private memberService = inject(MemberService);
  private serviceCategoryService = inject(ServiceCategoryService);
  private taskService = inject(TaskService);
  private toast = inject(ToastService);
  protected accountService = inject(AccountService);
  protected member = signal<Member | undefined>(undefined);
  protected serviceCategories = signal<ServiceCategory[]>([]);
  protected transactions = signal<TimeTransaction[]>([]);
  protected editMode = signal(false);
  protected availabilityEditMode = signal(false);
  protected avatarUploadMode = signal(false);
  protected avatarUploadLoading = signal(false);
  protected editableMember: EditableMember = {
    displayName: '',
    about: '',
    city: '',
    countryCode: '',
    isProfilePublic: true,
  };
  protected newSkill: MemberServiceCategoryEdit = {
    serviceCategoryId: 0,
    note: '',
  };
  protected newNeed: MemberServiceCategoryEdit = {
    serviceCategoryId: 0,
    note: '',
  };
  protected newAvailabilitySlot: MemberAvailabilitySlotEdit = {
    dayOfWeek: 1,
    startHour: 9,
    endHour: 10,
    mode: 3,
  };
  protected days = [
    { value: 0, label: 'Sunday' },
    { value: 1, label: 'Monday' },
    { value: 2, label: 'Tuesday' },
    { value: 3, label: 'Wednesday' },
    { value: 4, label: 'Thursday' },
    { value: 5, label: 'Friday' },
    { value: 6, label: 'Saturday' },
  ];
  protected startHours = Array.from({ length: 24 }, (_, hour) => hour);
  protected endHours = Array.from({ length: 24 }, (_, index) => index + 1);
  protected availabilityModes = [
    { value: 1, label: 'In person' },
    { value: 2, label: 'Remote' },
    { value: 3, label: 'Either' },
  ];

  ngOnInit(): void {
    this.route.data.subscribe({
      next: (data) => {
        const member = data['member'] as Member;
        this.member.set(member);
        this.editMode.set(false);
        this.availabilityEditMode.set(false);
        this.avatarUploadMode.set(false);
        this.setEditableMember(member);
        this.loadServiceCategoriesForOwnProfile(member);
        this.loadTransactionsForOwnProfile(member);
      },
    });
  }

  protected dayName(slot: MemberAvailabilitySlot) {
    return ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][slot.dayOfWeek] ?? 'Flexible';
  }

  protected modeName(slot: MemberAvailabilitySlot) {
    return {
      1: 'In person',
      2: 'Remote',
      3: 'Either',
    }[slot.mode] ?? 'Flexible';
  }

  protected isOwnProfile(member: Member) {
    return member.id === this.accountService.currentUser()?.id;
  }

  protected balance() {
    const userId = this.accountService.currentUser()?.id;

    return this.transactions().reduce((total, transaction) => {
      if (transaction.toMember.id === userId) return total + transaction.hours;
      if (transaction.fromMember.id === userId) return total - transaction.hours;

      return total;
    }, 0);
  }

  protected transactionDirection(transaction: TimeTransaction) {
    return transaction.toMember.id === this.accountService.currentUser()?.id ? 'Earned' : 'Spent';
  }

  protected availableSkillCategories(member: Member) {
    const selectedCategoryIds = new Set(member.offeredSkills.map((skill) => skill.serviceCategoryId));

    return this.serviceCategories().filter((category) => !selectedCategoryIds.has(category.id));
  }

  protected availableNeedCategories(member: Member) {
    const selectedCategoryIds = new Set(member.needsHelpWith.map((need) => need.serviceCategoryId));

    return this.serviceCategories().filter((category) => !selectedCategoryIds.has(category.id));
  }

  protected startEdit(member: Member) {
    this.setEditableMember(member);
    this.editMode.set(true);
  }

  protected cancelEdit(member: Member) {
    if (this.editForm?.dirty && !confirm('Discard unsaved profile changes?')) return;

    this.setEditableMember(member);
    this.editForm?.resetForm(this.editableMember);
    this.editMode.set(false);
  }

  protected updateProfile() {
    const member = this.member();

    if (!member) return;

    const editableMember = {
      ...this.editableMember,
      countryCode: this.editableMember.countryCode?.trim().toUpperCase(),
    };

    this.memberService.updateMember(editableMember).subscribe({
      next: () => {
        const updatedMember = {
          ...member,
          ...editableMember,
        };
        const currentUser = this.accountService.currentUser();

        if (currentUser && currentUser.displayName !== updatedMember.displayName) {
          this.accountService.setCurrentUser({
            ...currentUser,
            displayName: updatedMember.displayName,
          });
        }

        this.member.set(updatedMember);
        this.editableMember = editableMember;
        this.editForm?.resetForm(this.editableMember);
        this.editMode.set(false);
        this.toast.success('Profile updated');
      },
    });
  }

  protected addSkill() {
    if (!this.newSkill.serviceCategoryId) return;

    this.memberService.addSkill(this.newSkill).subscribe({
      next: (member) => {
        this.member.set(member);
        this.newSkill = { serviceCategoryId: 0, note: '' };
        this.toast.success('Skill added');
      },
    });
  }

  protected deleteSkill(skillId: number) {
    this.memberService.deleteSkill(skillId).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Skill removed');
      },
    });
  }

  protected addNeed() {
    if (!this.newNeed.serviceCategoryId) return;

    this.memberService.addNeed(this.newNeed).subscribe({
      next: (member) => {
        this.member.set(member);
        this.newNeed = { serviceCategoryId: 0, note: '' };
        this.toast.success('Need added');
      },
    });
  }

  protected deleteNeed(needId: number) {
    this.memberService.deleteNeed(needId).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Need removed');
      },
    });
  }

  protected addAvailabilitySlot() {
    if (this.newAvailabilitySlot.endHour <= this.newAvailabilitySlot.startHour) {
      this.toast.error('End hour must be after start hour');
      return;
    }

    this.memberService.addAvailabilitySlot(this.newAvailabilitySlot).subscribe({
      next: (member) => {
        this.member.set(member);
        this.newAvailabilitySlot = {
          dayOfWeek: this.newAvailabilitySlot.dayOfWeek,
          startHour: 9,
          endHour: 10,
          mode: this.newAvailabilitySlot.mode,
        };
        this.toast.success('Availability added');
      },
    });
  }

  protected deleteAvailabilitySlot(slotId: number) {
    this.memberService.deleteAvailabilitySlot(slotId).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Availability removed');
      },
    });
  }

  protected openAvatarUpload(member: Member) {
    if (!this.isOwnProfile(member)) return;

    this.avatarUploadMode.set(true);
  }

  protected uploadAvatar(file: File) {
    this.avatarUploadLoading.set(true);
    this.memberService.uploadAvatar(file).subscribe({
      next: (member) => {
        const currentUser = this.accountService.currentUser();

        if (currentUser) {
          this.accountService.setCurrentUser({
            ...currentUser,
            imageUrl: member.avatarUrl,
          });
        }

        this.member.set(member);
        this.avatarUploadMode.set(false);
        this.avatarUploadLoading.set(false);
        this.toast.success('Avatar updated');
      },
      error: () => this.avatarUploadLoading.set(false),
    });
  }

  protected closeAvatarUpload() {
    if (this.avatarUploadLoading()) return;

    this.avatarUploadMode.set(false);
  }

  private setEditableMember(member: Member) {
    this.editableMember = {
      displayName: member.displayName,
      about: member.about ?? '',
      city: member.city ?? '',
      countryCode: member.countryCode ?? '',
      isProfilePublic: member.isProfilePublic,
    };
  }

  private loadTransactionsForOwnProfile(member: Member) {
    if (!this.isOwnProfile(member)) {
      this.transactions.set([]);
      return;
    }

    this.taskService.getTransactions().subscribe({
      next: (transactions) => this.transactions.set(transactions),
    });
  }

  private loadServiceCategoriesForOwnProfile(member: Member) {
    if (!this.isOwnProfile(member) || this.serviceCategories().length > 0) return;

    this.serviceCategoryService.getServiceCategories().subscribe({
      next: (categories) => this.serviceCategories.set(categories),
    });
  }
}
