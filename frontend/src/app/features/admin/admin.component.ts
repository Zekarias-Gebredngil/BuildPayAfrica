import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RouterLink } from '@angular/router';

interface Overview { users: number; contractorsPending: number; products: number; orders: number; projects: number; deliveries: number; }
interface PendingVerification { id: string; userId: string; licenceNumber: string; status: string; documentUrls: string[]; }
interface Transaction { id: string; projectId: string; milestoneId?: string; amount: number; direction: string; createdAtUtc: string; }

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin.component.html'
})
export class AdminComponent {
  private readonly http = inject(HttpClient);

  overview      = signal<Overview | null>(null);
  verifications = signal<PendingVerification[]>([]);
  transactions  = signal<Transaction[]>([]);
  loading       = signal(true);
  error         = signal<string | null>(null);
  activeTab     = signal<'overview' | 'verifications' | 'transactions' | 'escrow'>('overview');
  processingId  = signal<string | null>(null);

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set(null);
    forkJoin({
      overview: this.http.get<Overview>('/api/admin/overview').pipe(catchError(() => of(null))),
      verifications: this.http.get<PendingVerification[]>('/api/contractors/verifications/pending').pipe(catchError(() => of([]))),
      transactions: this.http.get<Transaction[]>('/api/payments/admin/transactions').pipe(catchError(() => of([])))
    }).subscribe(({ overview, verifications, transactions }) => {
      this.overview.set(overview);
      this.verifications.set(verifications as PendingVerification[]);
      this.transactions.set(transactions as Transaction[]);
      this.loading.set(false);
    });
  }

  setVerificationStatus(id: string, status: 'Approved' | 'Rejected', reason?: string) {
    this.processingId.set(id);
    this.http.put(`/api/contractors/verifications/${id}/status`, { status, rejectionReason: reason ?? '' }).subscribe({
      next:  () => { this.processingId.set(null); this.load(); },
      error: (e) => { this.processingId.set(null); this.error.set(e?.error?.message ?? 'Update failed'); }
    });
  }

  get totalRevenue() {
    return this.transactions().filter(t => t.direction === 'Credit').reduce((s, t) => s + t.amount, 0);
  }
}
