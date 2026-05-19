import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

type Overview = {
  users: number;
  contractorsPending: number;
  products: number;
  orders: number;
  projects: number;
  deliveries: number;
};

type PendingContractor = {
  id: string;
  userId: string;
  licenseNumber: string;
  rating: number;
  verificationStatus: string;
};

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid gap-6">
      <div class="rounded-2xl border bg-white p-6">
        <div class="flex items-center justify-between gap-3">
          <div>
            <h2 class="text-xl font-semibold">Admin</h2>
            <p class="mt-1 text-sm text-slate-600">Overview and verifications.</p>
          </div>
          <button class="rounded-md border px-3 py-1.5 text-sm hover:bg-slate-100" (click)="load()">Refresh</button>
        </div>
      </div>

      <div *ngIf="overview" class="grid gap-3 md:grid-cols-3">
        <div class="rounded-xl border bg-white p-4">
          <div class="text-xs text-slate-500">Users</div>
          <div class="text-2xl font-semibold">{{ overview.users }}</div>
        </div>
        <div class="rounded-xl border bg-white p-4">
          <div class="text-xs text-slate-500">Pending contractors</div>
          <div class="text-2xl font-semibold">{{ overview.contractorsPending }}</div>
        </div>
        <div class="rounded-xl border bg-white p-4">
          <div class="text-xs text-slate-500">Orders</div>
          <div class="text-2xl font-semibold">{{ overview.orders }}</div>
        </div>
      </div>

      <div class="rounded-2xl border bg-white p-6">
        <h3 class="text-sm font-semibold">Pending contractor verifications</h3>
        <div *ngIf="pending.length === 0" class="mt-3 text-sm text-slate-600">None.</div>

        <div class="mt-3 grid gap-3">
          <div *ngFor="let c of pending" class="rounded-xl border p-4">
            <div class="flex items-center justify-between gap-3">
              <div>
                <div class="text-sm font-semibold">{{ c.id }}</div>
                <div class="mt-1 text-xs text-slate-500">License: {{ c.licenseNumber }} • Rating: {{ c.rating }}</div>
              </div>
              <div class="flex gap-2">
                <button class="rounded-md bg-emerald-600 px-3 py-1.5 text-xs text-white" (click)="setStatus(c.id, 'Approved')">Approve</button>
                <button class="rounded-md bg-rose-600 px-3 py-1.5 text-xs text-white" (click)="setStatus(c.id, 'Rejected')">Reject</button>
              </div>
            </div>
          </div>
        </div>

        <div *ngIf="error" class="mt-3 rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>
      </div>
    </div>
  `
})
export class AdminComponent {
  private readonly http = inject(HttpClient);

  overview: Overview | null = null;
  pending: PendingContractor[] = [];
  error: string | null = null;

  constructor() {
    this.load();
  }

  load() {
    this.error = null;
    this.http.get<Overview>(`/api/admin/overview`).subscribe({
      next: (o) => (this.overview = o),
      error: () => (this.error = 'Failed to load overview')
    });

    this.http.get<PendingContractor[]>(`/api/admin/contractors/pending`).subscribe({
      next: (p) => (this.pending = p),
      error: () => (this.error = 'Failed to load pending contractors')
    });
  }

  setStatus(contractorId: string, status: 'Approved' | 'Rejected') {
    this.http.put(`/api/contractors/${contractorId}/verification-status`, { status }).subscribe({
      next: () => this.load(),
      error: () => (this.error = 'Failed to update contractor status')
    });
  }
}

