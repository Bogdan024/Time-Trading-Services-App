import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog-service';
import { GroupService } from '../../../core/services/group-service';
import { ReportService } from '../../../core/services/report-service';
import { ToastService } from '../../../core/services/toast-service';
import { CommunityGroup } from '../../../types/group';
import { ReportReason, ReportTargetType } from '../../../types/moderation';

@Component({
  selector: 'app-group-detail',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './group-detail.html',
  styleUrl: './group-detail.css',
})
export class GroupDetail implements OnInit {
  private confirmDialog = inject(ConfirmDialogService);
  private groupService = inject(GroupService);
  private reportService = inject(ReportService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected group = signal<CommunityGroup | null>(null);
  protected loading = signal(false);
  protected reporting = signal(false);
  protected reportDetails = '';

  ngOnInit() {
    const groupId = Number(this.route.snapshot.paramMap.get('id'));

    if (!groupId) {
      this.router.navigateByUrl('/groups');
      return;
    }

    this.loadGroup(groupId);
  }

  protected joinGroup(group: CommunityGroup) {
    this.loading.set(true);
    this.groupService.joinGroup(group.id).subscribe({
      next: (updatedGroup) => {
        this.group.set(updatedGroup);
        this.loading.set(false);
        this.toast.success('Joined group');
      },
      error: () => this.loading.set(false),
    });
  }

  protected async leaveGroup(group: CommunityGroup) {
    const confirmed = await this.confirmDialog.confirm(
      `Leave ${group.name}?`,
      'You can join again later if the group is still available.'
    );

    if (!confirmed) return;

    this.loading.set(true);
    this.groupService.leaveGroup(group.id).subscribe({
      next: () => {
        this.loading.set(false);
        this.toast.success('Left group');
        this.loadGroup(group.id);
      },
      error: () => this.loading.set(false),
    });
  }

  protected reportGroup(group: CommunityGroup) {
    this.reporting.set(true);
    this.reportService.createReport({
      targetType: ReportTargetType.Group,
      targetIntId: group.id,
      reason: ReportReason.Inappropriate,
      details: this.reportDetails.trim() || undefined,
    }).subscribe({
      next: () => {
        this.toast.success('Report submitted for review');
        this.reportDetails = '';
        this.reporting.set(false);
      },
      error: () => this.reporting.set(false),
    });
  }

  private loadGroup(groupId: number) {
    this.groupService.getGroup(groupId).subscribe({
      next: (group) => this.group.set(group),
      error: () => this.router.navigateByUrl('/groups'),
    });
  }
}


