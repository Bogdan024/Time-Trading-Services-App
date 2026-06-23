import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin-service';
import { ToastService } from '../../../core/services/toast-service';
import { PendingGroup } from '../../../types/group';
import { ModerationReport } from '../../../types/moderation';

@Component({
  selector: 'app-moderation-queue',
  imports: [FormsModule],
  templateUrl: './moderation-queue.html',
  styleUrl: './moderation-queue.css',
})
export class ModerationQueue implements OnInit {
  private adminService = inject(AdminService);
  private toast = inject(ToastService);
  protected readonly pendingGroups = signal<PendingGroup[]>([]);
  protected readonly pendingReports = signal<ModerationReport[]>([]);
  protected rejectionReason = '';
  protected reportNotes = '';

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
    this.resolveReport(report, 'dismiss');
  }

  markActioned(report: ModerationReport) {
    this.resolveReport(report, 'action');
  }

  private loadQueue() {
    this.adminService.getPendingGroups().subscribe({
      next: (groups) => this.pendingGroups.set(groups),
      error: (error) => this.toast.error(error.error ?? 'Failed to load pending groups'),
    });

    this.adminService.getPendingReports().subscribe({
      next: (reports) => this.pendingReports.set(reports),
      error: (error) => this.toast.error(error.error ?? 'Failed to load reports'),
    });
  }

  private resolveReport(report: ModerationReport, action: 'dismiss' | 'action') {
    const request = action === 'dismiss'
      ? this.adminService.dismissReport(report.id, this.reportNotes)
      : this.adminService.actionReport(report.id, this.reportNotes);

    request.subscribe({
      next: () => {
        this.pendingReports.update((reports) => reports.filter((x) => x.id !== report.id));
        this.reportNotes = '';
        this.toast.success(action === 'dismiss' ? 'Report dismissed' : 'Report marked actioned');
      },
      error: (error) => this.toast.error(error.error ?? 'Failed to resolve report'),
    });
  }
}
