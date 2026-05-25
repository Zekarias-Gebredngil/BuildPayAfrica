import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { RouterLink } from '@angular/router';

interface Overview { users: number; contractorsPending: number; products: number; orders: number; projects: number; deliveries: number; }

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class AdminDashboardComponent {
  private readonly http = inject(HttpClient);
  overview = signal<Overview | null>(null);
  loading  = signal(true);
  error    = signal<string | null>(null);

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<Overview>('/api/admin/overview').pipe(catchError(() => of(null)))
      .subscribe((overview) => {
        this.overview.set(overview);
        this.loading.set(false);
      });
  }
}
