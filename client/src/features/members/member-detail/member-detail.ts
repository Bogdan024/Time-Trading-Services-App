import { DatePipe } from '@angular/common';
import { Component, HostListener, inject, OnInit, signal, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { MemberService } from '../../../core/services/member-service';
import { ReviewService } from '../../../core/services/review-service';
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
  ServicePreferenceItem,
} from '../../../types/member';
import { MemberReview, ReviewSummary } from '../../../types/review';
import { TimeTransaction } from '../../../types/task';
import { UploadModal } from '../../../shared/upload-modal/upload-modal';
import { ServicePreferenceEditor } from '../../../shared/service-preference-editor/service-preference-editor';
import { AvailabilityEditor, AvailabilitySlotRemove } from '../../../shared/availability-editor/availability-editor';
import { dayName as formatDayName, modeName as formatModeName } from '../../../shared/member-profile-options';

@Component({
  selector: 'app-member-detail',
  imports: [AvailabilityEditor, DatePipe, FormsModule, RouterLink, ServicePreferenceEditor, UploadModal],
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
  private reviewService = inject(ReviewService);
  private serviceCategoryService = inject(ServiceCategoryService);
  private taskService = inject(TaskService);
  private toast = inject(ToastService);
  protected accountService = inject(AccountService);
  protected member = signal<Member | undefined>(undefined);
  protected serviceCategories = signal<ServiceCategory[]>([]);
  protected transactions = signal<TimeTransaction[]>([]);
  protected reviews = signal<MemberReview[]>([]);
  protected reviewSummary = signal<ReviewSummary>({ averageRating: 0, reviewCount: 0 });
  protected readonly reviewStars = [1, 2, 3, 4, 5];
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
        this.loadReviewsForProfile(member);
      },
    });
  }

  protected dayName(slot: MemberAvailabilitySlot) {
    return formatDayName(slot.dayOfWeek);
  }

  protected modeName(slot: MemberAvailabilitySlot) {
    return formatModeName(slot.mode);
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

  protected addSkill(skillToAdd: MemberServiceCategoryEdit) {
    this.memberService.addSkill(skillToAdd).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Skill added');
      },
    });
  }

  protected deleteSkill(skillToDelete: ServicePreferenceItem) {
    if (!skillToDelete.id) return;

    this.memberService.deleteSkill(skillToDelete.id).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Skill removed');
      },
    });
  }

  protected addNeed(needToAdd: MemberServiceCategoryEdit) {
    this.memberService.addNeed(needToAdd).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Need added');
      },
    });
  }

  protected deleteNeed(needToDelete: ServicePreferenceItem) {
    if (!needToDelete.id) return;

    this.memberService.deleteNeed(needToDelete.id).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Need removed');
      },
    });
  }

  protected addAvailabilitySlot(slot: MemberAvailabilitySlotEdit) {
    this.memberService.addAvailabilitySlot(slot).subscribe({
      next: (member) => {
        this.member.set(member);
        this.toast.success('Availability added');
      },
    });
  }

  protected deleteAvailabilitySlot(slotToDelete: AvailabilitySlotRemove) {
    if (!slotToDelete.slot.id) return;

    this.memberService.deleteAvailabilitySlot(slotToDelete.slot.id).subscribe({
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

  private loadReviewsForProfile(member: Member) {
    this.reviewService.getMemberReviewSummary(member.id).subscribe({
      next: (summary) => this.reviewSummary.set(summary),
    });

    this.reviewService.getMemberReviews(member.id).subscribe({
      next: (reviews) => this.reviews.set(reviews),
    });
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

