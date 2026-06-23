import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { PendingGroup } from '../../types/group';
import { ModerationReport } from '../../types/moderation';

export type AdminUser = {
  id: string;
  displayName: string;
  email: string;
  imageUrl?: string;
  roles: string[];
};

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getUsersWithRoles() {
    return this.http.get<AdminUser[]>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(userId: string, roles: string[]) {
    const params = new HttpParams().set('roles', roles.join(','));
    return this.http.post<string[]>(this.baseUrl + `admin/edit-roles/${userId}`, {}, { params });
  }

  getPendingGroups() {
    return this.http.get<PendingGroup[]>(this.baseUrl + 'moderation/groups/pending');
  }

  approveGroup(groupId: number) {
    return this.http.post(this.baseUrl + `moderation/groups/${groupId}/approve`, {});
  }

  rejectGroup(groupId: number, reason?: string) {
    return this.http.post(this.baseUrl + `moderation/groups/${groupId}/reject`, { reason });
  }

  getPendingReports() {
    return this.http.get<ModerationReport[]>(this.baseUrl + 'moderation/reports');
  }

  dismissReport(reportId: number, moderatorNotes?: string) {
    return this.http.post<ModerationReport>(this.baseUrl + `moderation/reports/${reportId}/dismiss`, { moderatorNotes });
  }

  actionReport(reportId: number, moderatorNotes?: string) {
    return this.http.post<ModerationReport>(this.baseUrl + `moderation/reports/${reportId}/action`, { moderatorNotes });
  }
}
