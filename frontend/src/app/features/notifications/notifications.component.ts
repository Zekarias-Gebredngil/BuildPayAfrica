import { CommonModule } from '@angular/common';
import { Component, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface NotificationDto {
  id: string;
  type: string;
  content: string;
  read: boolean;
  createdAtUtc: string;
}

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html'
})
export class NotificationsComponent {
  private readonly http = inject(HttpClient);

  notifications = signal<NotificationDto[]>([]);
  loading = signal(true);
  error   = signal<string | null>(null);
  filter  = signal<'all' | 'unread' | 'read'>('all');

  readonly filtered = computed(() => {
    const f = this.filter();
    const all = this.notifications();
    if (f === 'unread') return all.filter(n => !n.read);
    if (f === 'read')   return all.filter(n => n.read);
    return all;
  });

  readonly unreadCount = computed(() => this.notifications().filter(n => !n.read).length);

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<NotificationDto[]>('/api/notifications').subscribe({
      next:  (res) => { this.notifications.set(res); this.loading.set(false); },
      error: ()    => { this.loading.set(false); this.error.set('Failed to load notifications'); }
    });
  }

  markRead(id: string) {
    this.http.put(`/api/notifications/${id}/read`, {}).subscribe({
      next: () => {
        this.notifications.update(ns => ns.map(n => n.id === id ? { ...n, read: true } : n));
      },
      error: () => this.error.set('Failed to mark as read')
    });
  }

  markAllRead() {
    const unread = this.notifications().filter(n => !n.read);
    if (!unread.length) return;
    forkJoin(
      unread.map(n => this.http.put(`/api/notifications/${n.id}/read`, {}).pipe(catchError(() => of(null))))
    ).subscribe(() => this.load());
  }

  typeIcon(type: string): string {
    const map: Record<string, string> = {
      'contractor_verified': '✅', 'contractor_rejected': '❌',
      'milestone_completed': '🎯', 'payment_released': '💰',
      'order_created': '📦', 'delivery_updated': '🚚', 'delivery_completed': '✅'
    };
    return map[type] ?? '🔔';
  }
}
