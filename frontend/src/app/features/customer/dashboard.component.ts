import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { forkJoin } from 'rxjs';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html'
})
export class CustomerDashboardComponent {
  private readonly http = inject(HttpClient);
  readonly auth = inject(AuthService);

  loading = signal(true);
  projects = signal<any[]>([]);
  notifications = signal<any[]>([]);
  recentOrders = signal<any[]>([]);

  readonly displayName = this.auth.displayName$;

  constructor() {
    forkJoin({
      projects: this.http.get<any[]>('/api/projects/my').pipe(catchError(() => of([]))),
      notifications: this.http.get<any[]>('/api/notifications').pipe(catchError(() => of([]))),
      orders: this.http.get<any[]>('/api/orders/history').pipe(catchError(() => of([])))
    }).subscribe(({ projects, notifications, orders }) => {
      this.projects.set(projects);
      this.notifications.set(notifications.filter((n: any) => !n.read).slice(0, 5));
      this.recentOrders.set(orders.slice(0, 3));
      this.loading.set(false);
    });
  }

  get activeProjects() { return this.projects().filter(p => p.status === 'Active').length; }
  get draftProjects()  { return this.projects().filter(p => p.status === 'Draft').length; }
  get unreadCount()    { return this.notifications().length; }
}
