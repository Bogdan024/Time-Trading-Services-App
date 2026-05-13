import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AccountService } from '../../../core/services/account-service';
import { TaskService } from '../../../core/services/task-service';
import { Member, MemberAvailabilitySlot } from '../../../types/member';
import { TimeTransaction } from '../../../types/task';

@Component({
  selector: 'app-member-detail',
  imports: [DatePipe, RouterLink],
  templateUrl: './member-detail.html',
  styleUrl: './member-detail.css',
})
export class MemberDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private taskService = inject(TaskService);
  protected accountService = inject(AccountService);
  protected member = signal<Member | undefined>(undefined);
  protected transactions = signal<TimeTransaction[]>([]);

  ngOnInit(): void {
    this.route.data.subscribe({
      next: (data) => {
        const member = data['member'] as Member;
        this.member.set(member);
        this.loadTransactionsForOwnProfile(member);
      },
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

  protected isOwnProfile(member: Member) {
    return member.id === this.accountService.currentUser()?.id;
  }

  protected balance() {
    const userId = this.accountService.currentUser()?.id;

    return this.transactions().reduce((total, transaction) => {
      if (transaction.toMember.id === userId) return total + transaction.hours;
      if (transaction.fromMember.id === userId) return total - transaction.hours;

      return total;
    }, 0);
  }

  protected transactionDirection(transaction: TimeTransaction) {
    return transaction.toMember.id === this.accountService.currentUser()?.id ? 'Earned' : 'Spent';
  }

  private loadTransactionsForOwnProfile(member: Member) {
    if (!this.isOwnProfile(member)) {
      this.transactions.set([]);
      return;
    }

    this.taskService.getTransactions().subscribe({
      next: (transactions) => this.transactions.set(transactions),
    });
  }
}
