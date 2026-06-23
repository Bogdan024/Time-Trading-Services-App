import { inject, Injectable, signal } from '@angular/core';
import { TimeTransaction } from '../../types/task';
import { AccountService } from './account-service';
import { TaskService } from './task-service';

@Injectable({
  providedIn: 'root',
})
export class BalanceService {
  private accountService = inject(AccountService);
  private taskService = inject(TaskService);
  private readonly transactions = signal<TimeTransaction[]>([]);

  readonly balance = signal(0);

  refreshBalance() {
    if (!this.accountService.currentUser()) {
      this.reset();
      return;
    }

    this.taskService.getTransactions().subscribe({
      next: (transactions) => {
        this.transactions.set(transactions);
        this.balance.set(this.calculateBalance(transactions));
      },
    });
  }

  reset() {
    this.transactions.set([]);
    this.balance.set(0);
  }

  private calculateBalance(transactions: TimeTransaction[]) {
    const userId = this.accountService.currentUser()?.id;

    return transactions.reduce((total, transaction) => {
      if (transaction.toMember.id === userId) return total + transaction.hours;
      if (transaction.fromMember.id === userId) return total - transaction.hours;

      return total;
    }, 0);
  }
}
