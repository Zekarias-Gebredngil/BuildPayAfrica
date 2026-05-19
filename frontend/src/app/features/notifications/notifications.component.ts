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
  template: `
    <div class="grid gap-6">
      <div class="rounded-2xl border bg-white p-6">
        <div class="flex items-center justify-between gap-3">
          <div>
            <h2 class="text-xl font-semibold">Notifications</h2>
            <p class="mt-1 text-sm text-slate-600">In-app notifications.</p>
          </div>
          <button class="rounded-md border px-3 py-1.5 text-sm hover:bg-slate-100" (click)="load()">Refresh</button>
        </div>
      </div>

      <div *ngIf="loading" class="text-sm text-slate-600">Loading…</div>
      <div *ngIf="error" class="rounded-md border border-red-200 bg-red-50 p-2 text-sm text-red-700">{{ error }}</div>

      <div class="grid gap-3">
        <div *ngFor="let n of notifications" class="rounded-2xl border bg-white p-5">
          <div class="flex items-start justify-between gap-3">
            <div>
              <div class="text-sm font-semibold">{{ n.title }}</div>
              <div class="mt-1 text-sm text-slate-700">{{ n.message }}</div>
              <div class="mt-2 text-xs text-slate-500">{{ n.createdAt | date : 'medium' }}</div>
            </div>
            <div class="flex items-center gap-2">
              <span class="text-xs" [class.text-slate-400]="n.isRead" [class.text-emerald-700]="!n.isRead">
                {{ n.isRead ? 'Read' : 'New' }}
              </span>
              <button
                *ngIf="!n.isRead"
                class="rounded-md border px-2 py-1 text-xs hover:bg-slate-100"
                (click)="markRead(n.id)"
              >
                Mark read
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `
})
export class NotificationsComponent {
  private readonly http = inject(HttpClient);

  notifications: NotificationDto[] = [];
  loading = true;
  error: string | null = null;

  constructor() {
    this.load();
  }

  load() {
    this.loading = true;
    this.error = null;
    this.http.get<NotificationDto[]>(`/api/notifications/my`).subscribe({
      next: (res) => {
        this.notifications = res;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Failed to load notifications';
      }
    });
  }

  markRead(id: string) {
    this.http.post(`/api/notifications/${id}/read`, {}).subscribe({
      next: () => this.load(),
      error: () => (this.error = 'Failed to mark read')
    });
  }
}

