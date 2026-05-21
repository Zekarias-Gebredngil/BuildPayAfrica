import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-contractor-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class ContractorDashboardComponent {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);
  readonly displayName = this.auth.displayName$;

  loading       = signal(true);
  verification  = signal<any>(null);
  notifications = signal<any[]>([]);

  constructor() {
    forkJoin({
      verification: this.http.get<any>('/api/contractors/verifications/me').pipe(catchError(() => of(null))),
      notifications: this.http.get<any[]>('/api/notifications').pipe(catchError(() => of([])))
    }).subscribe(({ verification, notifications }) => {
      this.verification.set(verification);
      this.notifications.set((notifications as any[]).filter((n: any) => !n.read).slice(0, 5));
      this.loading.set(false);
    });
  }

  get verificationStatus() { return this.verification()?.status ?? 'Not submitted'; }
  get isVerified()         { return this.verificationStatus === 'Approved'; }
  get unreadCount()        { return this.notifications().length; }
}
