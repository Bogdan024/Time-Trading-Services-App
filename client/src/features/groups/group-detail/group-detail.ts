import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { GroupService } from '../../../core/services/group-service';
import { ToastService } from '../../../core/services/toast-service';
import { CommunityGroup } from '../../../types/group';

@Component({
  selector: 'app-group-detail',
  imports: [DatePipe, RouterLink],
  templateUrl: './group-detail.html',
  styleUrl: './group-detail.css',
})
export class GroupDetail implements OnInit {
  private groupService = inject(GroupService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  protected group = signal<CommunityGroup | null>(null);
  protected loading = signal(false);

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

  protected leaveGroup(group: CommunityGroup) {
    if (!confirm('Are you sure you want to leave this group?')) return;

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

  private loadGroup(groupId: number) {
    this.groupService.getGroup(groupId).subscribe({
      next: (group) => this.group.set(group),
      error: () => this.router.navigateByUrl('/groups'),
    });
  }
}
