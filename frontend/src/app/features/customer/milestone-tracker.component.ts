import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

interface MilestoneWithProject {
  id: string; title: string; amount: number; dueDate: string; status: string;
  projectId: string; projectName: string;
}

@Component({
  selector: 'app-milestone-tracker',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './milestone-tracker.component.html'
})
export class MilestoneTrackerComponent {
  private readonly http = inject(HttpClient);

  milestones = signal<MilestoneWithProject[]>([]);
  loading    = signal(true);
  error      = signal<string | null>(null);
  filter     = signal<'all' | 'Pending' | 'Completed' | 'Approved'>('all');

  constructor() { this.load(); }

  load() {
    this.loading.set(true);
    this.http.get<any[]>('/api/projects/my').pipe(
      switchMap(projects => {
        if (!projects.length) return of([]);
        return forkJoin(
          projects.map(p =>
            this.http.get<any[]>(`/api/projects/${p.id}/milestones`).pipe(
              catchError(() => of([])),
              switchMap((ms: any[]) => of(ms.map((m: any) => ({ ...m, projectId: p.id, projectName: p.name }))))
            )
          )
        );
      })
    ).subscribe({
      next: (groups: any) => {
        const flat = (groups as any[][]).flat();
        flat.sort((a: any, b: any) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime());
        this.milestones.set(flat);
        this.loading.set(false);
      },
      error: () => { this.loading.set(false); this.error.set('Failed to load milestones'); }
    });
  }

  get filtered() {
    const f = this.filter();
    return f === 'all' ? this.milestones() : this.milestones().filter(m => m.status === f);
  }

  statusBadge(s: string) {
    return s === 'Approved' ? 'bp-badge-green' : s === 'Completed' ? 'bp-badge-blue' : 'bp-badge-gray';
  }

  isOverdue(dueDate: string, status: string) {
    return status === 'Pending' && new Date(dueDate) < new Date();
  }
}
