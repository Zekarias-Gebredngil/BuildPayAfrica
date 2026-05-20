import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

type NotificationDto = {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
};

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
})
export class NotificationsComponent {
  private readonly http = inject(HttpClient);

  notifications: NotificationDto[] = [];
  loading = true;
  error: string | null = null;
  filter: 'all' | 'unread' | 'read' = 'all';

  constructor() { this.load(); }

  load() {
    this.loading = true;
    this.error = null;
    this.http.get<NotificationDto[]>(`/api/notifications/my`).subscribe({
      next:  (res) => { this.notifications = res; this.loading = false; },
      error: ()    => { this.loading = false; this.error = 'Failed to load notifications'; }
    });
  }

  markRead(id: string) {
    this.http.post(`/api/notifications/${id}/read`, {}).subscribe({
      next: () => {
        // Update locally — no full reload needed for a single mark-read
        const n = this.notifications.find(x => x.id === id);
        if (n) n.isRead = true;
      },
      error: () => (this.error = 'Failed to mark as read')
    });
  }

  markAllRead() {
    const unread = this.notifications.filter(n => !n.isRead);
    if (!unread.length) return;

    // Fire all in parallel, then do a single reload when all complete
    forkJoin(
      unread.map(n =>
        this.http.post(`/api/notifications/${n.id}/read`, {}).pipe(
          catchError(() => of(null))  // don't let one failure abort the rest
        )
      )
    ).subscribe(() => this.load());
  }

  get filtered(): NotificationDto[] {
    if (this.filter === 'unread') return this.notifications.filter(n => !n.isRead);
    if (this.filter === 'read')   return this.notifications.filter(n => n.isRead);
    return this.notifications;
  }

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }
}
