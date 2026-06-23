import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { GroupService } from '../../../core/services/group-service';
import { ToastService } from '../../../core/services/toast-service';
import { LocationPicker } from '../../../shared/location-picker/location-picker';
import { CommunityGroup, CreateGroup } from '../../../types/group';
import { TaskLocation } from '../../../types/task';

@Component({
  selector: 'app-group-list',
  imports: [FormsModule, LocationPicker, RouterLink],
  templateUrl: './group-list.html',
  styleUrl: './group-list.css',
})
export class GroupList implements OnInit {
  private groupService = inject(GroupService);
  private toast = inject(ToastService);

  protected groups = signal<CommunityGroup[]>([]);
  protected loading = signal(false);
  protected showCreate = signal(false);
  protected groupForm: CreateGroup = this.emptyForm();

  ngOnInit() {
    this.loadGroups();
  }

  protected createGroup() {
    if (!this.canCreateGroup()) return;

    this.loading.set(true);
    this.groupService.createGroup(this.normalizedForm()).subscribe({
      next: (group) => {
        this.groups.update((groups) => [group, ...groups]);
        this.groupForm = this.emptyForm();
        this.showCreate.set(false);
        this.loading.set(false);
        this.toast.success('Group created');
      },
      error: () => this.loading.set(false),
    });
  }

  protected joinGroup(group: CommunityGroup) {
    this.groupService.joinGroup(group.id).subscribe({
      next: (updatedGroup) => {
        this.replaceGroup(updatedGroup);
        this.toast.success('Joined group');
      },
    });
  }

  protected canCreateGroup() {
    return this.groupForm.name.trim().length >= 3
      && this.groupForm.description.trim().length >= 10
      && !!this.groupForm.city?.trim()
      && !!this.groupForm.countryCode?.trim();
  }

  protected onGroupLocationSelected(location: TaskLocation) {
    this.groupForm.city = location.city;
    this.groupForm.countryCode = location.countryCode;
  }

  protected onGroupLocationCleared() {
    this.groupForm.city = '';
    this.groupForm.countryCode = '';
  }

  private loadGroups() {
    this.groupService.getGroups().subscribe({
      next: (groups) => this.groups.set(groups),
    });
  }

  private replaceGroup(group: CommunityGroup) {
    this.groups.update((groups) => groups.map((item) => (item.id === group.id ? group : item)));
  }

  private normalizedForm(): CreateGroup {
    return {
      name: this.groupForm.name.trim(),
      description: this.groupForm.description.trim(),
      theme: this.groupForm.theme?.trim() || undefined,
      city: this.groupForm.city?.trim() || undefined,
      countryCode: this.groupForm.countryCode?.trim().toUpperCase() || undefined,
    };
  }

  private emptyForm(): CreateGroup {
    return {
      name: '',
      description: '',
      theme: '',
      city: '',
      countryCode: '',
    };
  }
}
