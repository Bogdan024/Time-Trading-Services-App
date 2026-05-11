import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Member, MemberAvailabilitySlot } from '../../../types/member';

@Component({
  selector: 'app-member-detail',
  imports: [DatePipe, RouterLink],
  templateUrl: './member-detail.html',
  styleUrl: './member-detail.css',
})
export class MemberDetail implements OnInit {
  private route = inject(ActivatedRoute);
  protected member = signal<Member | undefined>(undefined);

  ngOnInit(): void {
    this.route.data.subscribe({
      next: (data) => this.member.set(data['member']),
    });
  }

  protected dayName(slot: MemberAvailabilitySlot) {
    return ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'][slot.dayOfWeek] ?? 'Flexible';
  }

  protected modeName(slot: MemberAvailabilitySlot) {
    return {
      1: 'In person',
      2: 'Remote',
      3: 'Either',
    }[slot.mode] ?? 'Flexible';
  }
}
