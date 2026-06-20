import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  EditableMember,
  Member,
  MemberAvailabilitySlotEdit,
  MemberProfileSetup,
  MemberServiceCategoryEdit,
} from '../../types/member';

@Injectable({
  providedIn: 'root',
})
export class MemberService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'members');
  }

  getMember(id: string) {
    return this.http.get<Member>(this.baseUrl + 'members/' + id);
  }

  updateMember(member: EditableMember) {
    return this.http.put<void>(this.baseUrl + 'members', member);
  }

  addSkill(skill: MemberServiceCategoryEdit) {
    return this.http.post<Member>(this.baseUrl + 'members/skills', skill);
  }

  deleteSkill(skillId: number) {
    return this.http.delete<Member>(this.baseUrl + 'members/skills/' + skillId);
  }

  addNeed(need: MemberServiceCategoryEdit) {
    return this.http.post<Member>(this.baseUrl + 'members/needs', need);
  }

  deleteNeed(needId: number) {
    return this.http.delete<Member>(this.baseUrl + 'members/needs/' + needId);
  }

  addAvailabilitySlot(slot: MemberAvailabilitySlotEdit) {
    return this.http.post<Member>(this.baseUrl + 'members/availability', slot);
  }

  deleteAvailabilitySlot(slotId: number) {
    return this.http.delete<Member>(this.baseUrl + 'members/availability/' + slotId);
  }

  saveOnboardingProfileSetup(setup: MemberProfileSetup) {
    const requests = [
      ...setup.skills.map((skill) => this.addSkill(skill)),
      ...setup.needs.map((need) => this.addNeed(need)),
      ...setup.availabilitySlots.map((slot) => this.addAvailabilitySlot(slot)),
    ];

    return requests.length > 0 ? forkJoin(requests) : of([]);
  }

  uploadAvatar(file: File) {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<Member>(this.baseUrl + 'members/avatar', formData);
  }
}
