import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

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
}
