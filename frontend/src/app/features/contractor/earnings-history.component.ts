import { CommonModule } from '@angular/common';
import { Component, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Transaction { id: string; projectId: string; milestoneId?: string; amount: number; direction: string; createdAtUtc: string; }

@Component({
  selector: 'app-earnings-history',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './earnings-history.component.html'
})
export class EarningsHistoryComponent {
  private readonly http = inject(HttpClient);
  transactions = signal<Transaction[]>([]);
  loading = signal(true);
  error   = signal<string | null>(null);

  readonly totalEarned = computed(() =>
    this.transactions().filter(t => t.direction === 'Credit').reduce((s, t) => s + t.amount, 0));

  constructor() {
    // Contractors see their released payments via admin transactions endpoint
    // For now show a placeholder — full earnings requires a contractor-specific endpoint
    this.loading.set(false);
  }
}
