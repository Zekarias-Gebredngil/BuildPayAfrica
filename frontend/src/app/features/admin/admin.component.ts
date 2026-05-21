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
  licenceNumber: string;
  status: string;
  documentUrls: string[];
};

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.component.html'
})
export class AdminComponent {
  private readonly http = inject(HttpClient);

  overview: Overview | null = null;
  pending: PendingContractor[] = [];
  error: string | null = null;
  loading = true;

  activeTab: 'overview' | 'verifications' | 'users' | 'transactions' = 'overview';

  constructor() { this.load(); }

  load() {
    this.loading = true;
    this.error = null;

    this.http.get<Overview>(`/api/admin/overview`).subscribe({
      next:  (o) => { this.overview = o; this.loading = false; },
      error: ()  => { this.error = 'Failed to load overview'; this.loading = false; }
    });

    this.http.get<PendingContractor[]>(`/api/contractors/verifications/pending`).subscribe({
      next:  (p) => (this.pending = p),
      error: ()  => (this.error = 'Failed to load pending contractors')
    });
  }

  setStatus(verificationId: string, status: 'Approved' | 'Rejected') {
    this.http.put(`/api/contractors/verifications/${verificationId}/status`, { status, rejectionReason: status === 'Rejected' ? 'Rejected by admin' : null }).subscribe({
      next:  () => this.load(),
      error: ()  => (this.error = 'Failed to update contractor status')
    });
  }
}
