import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface Transaction { id: string; projectId: string; milestoneId?: string; amount: number; direction: string; createdAtUtc: string; }

@Component({
  selector: 'app-admin-transactions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transactions.component.html'
})
export class AdminTransactionsComponent {
  private readonly http = inject(HttpClient);

  transactions  = signal<Transaction[]>([]);
  loading       = signal(true);
  error         = signal<string | null>(null);

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<Transaction[]>('/api/payments/admin/transactions').pipe(catchError(() => of([])))
      .subscribe((transactions) => {
        this.transactions.set(transactions);
        this.loading.set(false);
      });
  }

  get totalRevenue() {
    return this.transactions().filter(t => t.direction === 'Credit').reduce((s, t) => s + t.amount, 0);
  }
}
