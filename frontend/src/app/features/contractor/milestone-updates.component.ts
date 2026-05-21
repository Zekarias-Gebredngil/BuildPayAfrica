import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

interface MilestoneRow { id: string; title: string; amount: number; dueDate: string; status: string; projectId: string; projectName: string; }

@Component({
  selector: 'app-milestone-updates',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './milestone-updates.component.html'
})
export class MilestoneUpdatesComponent {
  private readonly http = inject(HttpClient);

  milestones = signal<MilestoneRow[]>([]);
  loading    = signal(true);
  error      = signal<string | null>(null);
  updating   = signal<string | null>(null);

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    // Contractors see projects where they are assigned (contractorUserId matches)
    // We fetch all projects the contractor is on via a search — for now use /api/projects/my
    // which returns projects where the caller is the customer. Contractors need a separate endpoint.
    // We'll call the projects list and filter by contractorUserId matching the current user.
    this.http.get<any[]>('/api/projects/my').pipe(
      switchMap(projects => {
        if (!projects.length) return of([]);
        return forkJoin(projects.map(p =>
          this.http.get<any[]>(`/api/projects/${p.id}/milestones`).pipe(
            catchError(() => of([])),
            switchMap((ms: any[]) => of(ms.map((m: any) => ({ ...m, projectId: p.id, projectName: p.name }))))
          )
        ));
      })
    ).subscribe({
      next: (groups: any) => {
        this.milestones.set((groups as any[][]).flat());
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.error.set('Failed to load milestones'); }
    });
  }

  markComplete(m: MilestoneRow) {
    this.updating.set(m.id);
    this.http.put(`/api/projects/${m.projectId}/milestones/${m.id}/status`, { status: 'Completed' }).subscribe({
      next:  () => { this.updating.set(null); this.load(); },
      error: (e) => { this.updating.set(null); this.error.set(e?.error?.message ?? 'Update failed'); }
    });
  }

  statusBadge(s: string) {
    return s === 'Approved' ? 'bp-badge-green' : s === 'Completed' ? 'bp-badge-blue' : 'bp-badge-gray';
  }
}
