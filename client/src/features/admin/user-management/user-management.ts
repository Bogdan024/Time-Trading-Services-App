import { Component, inject, OnInit, signal } from '@angular/core';
import { AdminService, AdminUser } from '../../../core/services/admin-service';
import { ToastService } from '../../../core/services/toast-service';

@Component({
  selector: 'app-user-management',
  imports: [],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
})
export class UserManagement implements OnInit {
  private adminService = inject(AdminService);
  private toast = inject(ToastService);
  protected readonly users = signal<AdminUser[]>([]);
  protected readonly selectedUser = signal<AdminUser | null>(null);
  protected readonly selectedRoles = signal<string[]>([]);
  protected readonly availableRoles = ['Member', 'Moderator', 'Admin'];

  ngOnInit() {
    this.loadUsers();
  }

  openRoleEditor(user: AdminUser) {
    this.selectedUser.set(user);
    this.selectedRoles.set([...user.roles]);
  }

  closeRoleEditor() {
    this.selectedUser.set(null);
    this.selectedRoles.set([]);
  }

  toggleRole(role: string, checked: boolean) {
    const roles = this.selectedRoles();
    this.selectedRoles.set(checked ? [...roles, role] : roles.filter((x) => x !== role));
  }

  updateRoles() {
    const user = this.selectedUser();

    if (!user) {
      return;
    }

    this.adminService.updateUserRoles(user.id, this.selectedRoles()).subscribe({
      next: (roles) => {
        this.users.update((users) => users.map((x) => (x.id === user.id ? { ...x, roles } : x)));
        this.toast.success('Roles updated');
        this.closeRoleEditor();
      },
      error: (error) => this.toast.error(error.error ?? 'Could not update roles'),
    });
  }

  private loadUsers() {
    this.adminService.getUsersWithRoles().subscribe({
      next: (users) => this.users.set(users),
      error: (error) => this.toast.error(error.error ?? 'Could not load users'),
    });
  }
}
