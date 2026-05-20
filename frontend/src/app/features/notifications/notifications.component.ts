import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

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
      next:  () => this.load(),
      error: ()  => (this.error = 'Failed to mark as read')
    });
  }

  markAllRead() {
    const unread = this.notifications.filter(n => !n.isRead);
    unread.forEach(n => this.markRead(n.id));
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
