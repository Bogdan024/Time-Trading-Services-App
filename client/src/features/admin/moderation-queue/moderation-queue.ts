import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../../core/services/admin-service';
import { AccountService } from '../../../core/services/account-service';
import { ToastService } from '../../../core/services/toast-service';
import { PendingGroup } from '../../../types/group';
import { ModerationReport } from '../../../types/moderation';

@Component({
  selector: 'app-moderation-queue',
  imports: [FormsModule, RouterLink],
  templateUrl: './moderation-queue.html',
  styleUrl: './moderation-queue.css',
})
export class ModerationQueue implements OnInit {
  private adminService = inject(AdminService);
  private accountService = inject(AccountService);
  private toast = inject(ToastService);
  protected readonly pendingGroups = signal<PendingGroup[]>([]);
  protected readonly pendingReports = signal<ModerationReport[]>([]);
  protected rejectionReason = '';

  protected isAdmin() {
    return this.accountService.hasRole(['Admin']);
  }

  ngOnInit() {
    this.loadQueue();
  }

  approveGroup(group: PendingGroup) {
    this.adminService.approveGroup(group.id).subscribe({
      next: () => {
        this.pendingGroups.update((groups) => groups.filter((x) => x.id !== group.id));
        this.toast.success('Group approved');
      },
      error: (error) => this.toast.error(error.error ?? 'Failed to approve group'),
    });
  }

  rejectGroup(group: PendingGroup) {
    this.adminService.rejectGroup(group.id, this.rejectionReason).subscribe({
      next: () => {
        this.pendingGroups.update((groups) => groups.filter((x) => x.id !== group.id));
        this.rejectionReason = '';
        this.toast.success('Group rejected');
      },
      error: (error) => this.toast.error(error.error ?? 'Failed to reject group'),
    });
  }

  dismissReport(report: ModerationReport) {
    this.adminService.dismissReport(report.id).subscribe({
      next: () => {
        this.removeReport(report.id);
        this.toast.success('Report dismissed');
      },
      error: (error) => this.toast.error(error.error ?? 'Failed to dismiss report'),
    });
  }

  cancelReportedTask(report: ModerationReport) {
    this.adminService.cancelReportedTask(report.id).subscribe({
      next: () => {
        this.removeReport(report.id);
        this.toast.success('Reported task cancelled');
      },
      error: (error) => this.toast.error(error.error ?? 'Failed to cancel reported task'),
    });
  }

  protected isTaskReport(report: ModerationReport) {
    return report.targetType === 'Task' && !!report.targetIntId;
  }

  private loadQueue() {
    if (this.isAdmin()) {
      this.adminService.getPendingGroups().subscribe({
        next: (groups) => this.pendingGroups.set(groups),
        error: (error) => this.toast.error(error.error ?? 'Failed to load pending groups'),
      });
    }

    this.adminService.getPendingReports().subscribe({
      next: (reports) => this.pendingReports.set(reports),
      error: (error) => this.toast.error(error.error ?? 'Failed to load reports'),
    });
  }

  private removeReport(reportId: number) {
    this.pendingReports.update((reports) => reports.filter((x) => x.id !== reportId));
  }
}
