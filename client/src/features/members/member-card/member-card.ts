import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Member } from '../../../types/member';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  member = input.required<Member>();

  protected categories(member: Member) {
    return member.offeredSkills.slice(0, 3).map((skill) => skill.serviceCategory.name);
  }
}
