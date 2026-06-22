import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { CommunityGroup, CreateGroup } from '../../types/group';

@Injectable({
  providedIn: 'root',
})
export class GroupService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getGroups() {
    return this.http.get<CommunityGroup[]>(this.baseUrl + 'groups');
  }

  getGroup(id: number) {
    return this.http.get<CommunityGroup>(this.baseUrl + 'groups/' + id);
  }

  createGroup(group: CreateGroup) {
    return this.http.post<CommunityGroup>(this.baseUrl + 'groups', group);
  }

  joinGroup(id: number) {
    return this.http.post<CommunityGroup>(this.baseUrl + 'groups/' + id + '/join', {});
  }

  leaveGroup(id: number) {
    return this.http.post<void>(this.baseUrl + 'groups/' + id + '/leave', {});
  }
}
